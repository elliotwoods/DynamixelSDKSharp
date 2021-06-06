using DynamixelSDKSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method = Method.GET)]
	[Serializable]
	class HomeAll : IRequest
	{
		public object Perform()
		{
			var servos = PortPool.X.Servos;

			// Go to home
			{
				var writeRequests = new List<WriteAsyncRequest>();

				foreach (var iterator in servos)
				{
					var servo = iterator.Value;
					//var value = servo.ReadValue(RegisterType.MinPositionLimit);
					writeRequests.Add(new WriteAsyncRequest
					{
						servo = servo
						,
						registerType = RegisterType.GoalPosition
					});
					servo.Registers[RegisterType.GoalPosition].Value = 2048;
				}
				PortPool.X.WriteAsync(writeRequests);
			}

			return new { };
		}
	}
}