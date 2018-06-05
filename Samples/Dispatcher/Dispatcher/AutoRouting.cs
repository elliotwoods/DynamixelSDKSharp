using DynamixelSDKSharp;
using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dispatcher
{
	public class AutoRouting : NancyModule
	{
		public class Route
		{
			public Requests.RequestHandlerAttribute RequestHandlerAttribute { get; set; }
			public Type RequestType;
		}

		public static Dictionary<string, Route> Routes { get; private set; } = null;

		public AutoRouting()
		{
            //Override default option route - sets CORS headers so that we can make cross domain requests.
            {
                Options("/", _ =>
                {
                    return new Response();
                });

                After.AddItemToEndOfPipeline((ctx) => ctx.Response
                .WithHeader("Access-Control-Allow-Origin", "*")
                .WithHeader("Access-Control-Allow-Methods", "POST,GET")
                .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type"));
            }
            

            //Add all IRequests to Routes
            {
				if (AutoRouting.Routes == null) // check that it's first pass
				{
					AutoRouting.Routes = new Dictionary<string, Route>();
					var assembly = this.GetType().GetTypeInfo().Assembly;

					//get all types inheriting from IRequest
					var types = assembly.GetTypes().Where(t => typeof(Requests.IRequest).IsAssignableFrom(t));
					foreach (var type in types)
					{
						if(type.Attributes.HasFlag(TypeAttributes.Abstract))
						{
							continue;
						}

						try
						{
							string address;
							{
								var assemblyAddress = type.FullName.Split('.').ToList();
								assemblyAddress.RemoveAt(0);
								assemblyAddress.RemoveAt(0);
								address = String.Join("/", assemblyAddress);
							}

							var attribute = type.GetCustomAttribute(typeof(Requests.RequestHandlerAttribute)) as Requests.RequestHandlerAttribute;
							if (attribute == null)
							{
								//if no attribute is set for this class, make a default one
								attribute = new Requests.RequestHandlerAttribute();
							} else
							{
								//handle custom addresses
								if (attribute.CustomAddress != null)
								{
									address = attribute.CustomAddress;
								}
							}

							//add the request to the AutoRouting routes table
							AutoRouting.Routes.Add(address, new Route
							{
								RequestHandlerAttribute = attribute,
								RequestType = type
							});
						}
						catch(Exception e)
						{
							Logger.Log<AutoRouting>(Logger.Level.Error, e);
						}
					}
				}
			}

			//Perform all Routes
			foreach (var route in AutoRouting.Routes)
			{
				if (route.Value.RequestHandlerAttribute.Method.HasFlag(Requests.Method.GET)) {
					Get(route.Key, args =>
						{
							return respond(() =>
							{
								//make an isntance of the request (no input body)
								var request = (Requests.IRequest)Activator.CreateInstance(route.Value.RequestType);
								return request.Perform();
							}, route.Value);
						});
				}
				if (route.Value.RequestHandlerAttribute.Method.HasFlag(Requests.Method.POST)) {

					Post(route.Key, args =>
					{
						return respond(() =>
						{
								//make an isntance of the request from the incoming request body
								var getRequestMethod = typeof(AutoRouting).GetMethod("getRequest");
							var getRequestMethodSpecific = getRequestMethod.MakeGenericMethod(route.Value.RequestType);
							var requestUntyped = getRequestMethodSpecific.Invoke(this, null);
							var request = (Requests.IRequest)requestUntyped;
							return request.Perform();
						}, route.Value);
					});
				}
			}
		}

		public T getRequest<T>() where T : new()
		{
			var incomingStream = Request.Body;
			var reader = new StreamReader(incomingStream, Encoding.UTF8);
			var requestString = reader.ReadToEnd();
			var request = JsonConvert.DeserializeObject<T>(requestString, ProductDatabase.JsonSerializerSettings);

			if (request == null)
			{
				request = new T();
			}

			return request;
		}

		public static object respond(Func<object> requestHandler, Route route)
		{
			object responseObject = null;

			try
			{
				//set the thread name
				{
					var thisThread = Thread.CurrentThread;
					if (thisThread.Name == null)
					{
						thisThread.Name = route.RequestType.ToString();
					}
				}

				//lock the PortPool and perform the request
				switch (route.RequestHandlerAttribute.ThreadUsage)
				{
					case Requests.ThreadUsage.Shared:
						PortPool.X.Lock.AcquireReaderLock(1000);
						try
						{
							responseObject = requestHandler();
						}
						catch(Exception e)
						{
							throw (e);
						}
						finally
						{
							PortPool.X.Lock.ReleaseReaderLock();
						}
						break;
					case Requests.ThreadUsage.Exclusive:
						PortPool.X.Lock.AcquireWriterLock(1000);
						try
						{
							responseObject = requestHandler();
						}
						catch (Exception e)
						{
							throw (e);
						}
						finally
						{
							PortPool.X.Lock.ReleaseWriterLock();
						}
						break;
				}

				return new TextResponse(JsonConvert.SerializeObject(new
				{
					success = true,
					data = responseObject
				}, ProductDatabase.JsonSerializerSettings)
				, "application/json");
			}
			catch (Exception e)
			{
				Logger.Log(Logger.Level.Error, e, route.RequestType);
				if (e is AggregateException)
				{
					var ae = e as AggregateException;
					foreach(var innerException in ae.InnerExceptions)
					{
						Logger.Log(Logger.Level.Error, innerException, route.RequestType);
					}
				}

				return new TextResponse(JsonConvert.SerializeObject(new
				{
					success = false,
					exception = new Utils.ExceptionMessage(e)
				}, ProductDatabase.JsonSerializerSettings)
				, "application/json");
			}
		}
	}
}
