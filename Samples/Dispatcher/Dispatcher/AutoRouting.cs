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
			public Type Type;
		}

		static Dictionary<string, Route> FRoutes = null;

		public AutoRouting()
		{
			//Add all IRequests to Routes
			{
				if (AutoRouting.FRoutes == null) // check that it's first pass
				{
					AutoRouting.FRoutes = new Dictionary<string, Route>();
					var assembly = this.GetType().GetTypeInfo().Assembly;
					var types = assembly.GetTypes();
					foreach (var type in types)
					{
						var attribute = type.GetCustomAttribute(typeof(Requests.RequestHandlerAttribute)) as Requests.RequestHandlerAttribute;
						if (attribute != null)
						{
							AutoRouting.FRoutes.Add(attribute.Address, new Route
							{
								RequestHandlerAttribute = attribute,
								Type = type
							});
						}
					}
				}
			}

			//Perform all Routes
			foreach (var route in AutoRouting.FRoutes)
			{
				if (route.Value.RequestHandlerAttribute.Method.HasFlag(Requests.Method.GET)) {
					Get(route.Key, args =>
						{
							return respond(() =>
							{
								//make an isntance of the request (no input body)
								var request = (Requests.IRequest)Activator.CreateInstance(route.Value.Type);
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
							var getRequestMethodSpecific = getRequestMethod.MakeGenericMethod(route.Value.Type);
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
						thisThread.Name = route.Type.ToString();
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
				Logger.Log(Logger.Level.Error, e, route.Type);

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
