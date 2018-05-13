using Nancy;
using Nancy.ModelBinding;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DynamixelSDKSharp;
using Nancy.Responses;

namespace Dispatcher
{
	public class Module : NancyModule
	{
		T getRequest<T>() where T : new()
		{
			var incomingStream = Request.Body;
			var reader = new StreamReader(incomingStream, Encoding.UTF8);
			var requestString = reader.ReadToEnd();
			var request = JsonConvert.DeserializeObject<T>(requestString, ProductDatabase.JsonSerializerSettings);

			// if there is no incoming json
			if(request == null)
			{
				request = new T();
			}

			return request;
		}

		object respond(Func<object> requestHandler)
		{
			object responseObject;

			try
			{
				responseObject = requestHandler();
				return new TextResponse(JsonConvert.SerializeObject(new
				{
					success = true,
					data = responseObject
				}, ProductDatabase.JsonSerializerSettings)
				, "application/json");
			}
			catch (Exception e)
			{
				Logger.Log(Logger.Level.Error, e);

				return new TextResponse(JsonConvert.SerializeObject(new
				{
					success = false,
					exception = new
					{
						message = e.Message,
						stackTrace = e.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None),
						source = e.Source
					}
				}, ProductDatabase.JsonSerializerSettings)
				, "application/json");
			}
		}

		object respond(Action requestHandler)
		{
			//convert the function into one which returns an empty object
			return respond(() =>
			{
				requestHandler();
				return new { };
			});
		}

		public Module()
		{
			Get("/", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.Refresh>();
					return request.Perform();
				});
			});

			Get("/refresh", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.Refresh>();
					return request.Perform();
				});
			});

			Get("/getPorts", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.GetPorts>();
					return request.Perform();
				});
			});

			Get("/getServos", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.GetServos>();
					return request.Perform();
				});
			});

			Post("/setRegisterValue", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.SetRegisterValue>();
					return request.Perform();
				});
			});

			Post("/getRegisterValue", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.GetRegisterValue>();
					return request.Perform();
				});
			});

			Post("/moveServo", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.MoveServo>();
					return request.Perform();
				});
			});

			Get("/initialiseAll", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.InitialiseAll>();
					return request.Perform();
				});
			});

			Get("/shutdownAll", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.ShutdownAll>();
					return request.Perform();
				});
			});

			Get("/checkSafetyConstraints", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.CheckSafetyConstraints>();
					return request.Perform();
				});
			});

			Get("/schedulerEnable", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.SchedulerEnable>();
					return request.Perform();
				});
			});

			Get("/schedulerDisable", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.SchedulerDisable>();
					return request.Perform();
				});
			});
		}
	}
}
