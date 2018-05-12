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
		T getRequest<T>()
		{
			var incomingStream = Request.Body;
			var reader = new StreamReader(incomingStream, Encoding.UTF8);
			var requestString = reader.ReadToEnd();
			return JsonConvert.DeserializeObject<T>(requestString, ProductDatabase.JsonSerializerSettings);
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
				Logger.Log(Logger.Level.Error, e.Message + Environment.NewLine + e.StackTrace);

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
				return "";
			});

			Get("/refresh", args =>
			{
				return respond(() =>
				{
					PortPool.X.Refresh();
				});
			});

			Get("/ports", args =>
			{
				return respond(() =>
				{
					return PortPool.X.Ports;
				});
			});

			Get("/servos", args =>
			{
				return respond(() =>
				{
					return PortPool.X.Servos;
				});
			});

			Post("/setRegister", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.SetRegister>();
					return request.Perform();
				});
			});

			Post("/getRegister", args =>
			{
				return respond(() =>
				{
					var request = getRequest<Requests.GetRegister>();
					return request.Perform();
				});
			});
		}
	}
}
