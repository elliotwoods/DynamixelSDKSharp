using DynamixelSDKSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	class WalkLimits : IRequest
	{
		double timeToSpendAtLimit = 10.0;
		int offset = 0;

		public object Perform()
		{
			var servos = PortPool.X.Servos;

			// Go to min
			{
				var writeRequests = new List<WriteAsyncRequest>();

				foreach(var iterator in servos)
				{
					var servo = iterator.Value;
					try
					{
						var value = servo.ReadValue(RegisterType.MinPositionLimit) + offset;
						writeRequests.Add(new WriteAsyncRequest
						{
							servo = servo
							, registerType = RegisterType.GoalPosition
						});
						servo.Registers[RegisterType.GoalPosition].Value = value;
					}
					catch (Exception e)
					{
						throw (new Exception(String.Format("Servo {0} : {1}", servo.ID, e.Message)));
					}
				}
				PortPool.X.WriteAsync(writeRequests);
			}

			// Wait at limit
			Thread.Sleep((int) (this.timeToSpendAtLimit * 1000.0));

			// Go to max
			{
				var writeRequests = new List<WriteAsyncRequest>();

				foreach (var iterator in servos)
				{
					var servo = iterator.Value;
					try
					{
						var value = servo.ReadValue(RegisterType.MaxPositionLimit) - offset;
						writeRequests.Add(new WriteAsyncRequest
						{
							servo = servo
							,
							registerType = RegisterType.GoalPosition
						});
						servo.Registers[RegisterType.GoalPosition].Value = value;
					}
					catch(Exception e)
					{
						throw (new Exception(String.Format("Servo {0} : {1}", servo.ID, e.Message)));
					}
					
				}
				PortPool.X.WriteAsync(writeRequests);
			}

			// Wait at limit
			Thread.Sleep((int)(this.timeToSpendAtLimit * 1000.0 * 2));

			// Go to center
			{
				var writeRequests = new List<WriteAsyncRequest>();

				foreach (var iterator in servos)
				{
					var servo = iterator.Value;
					var min = servo.ReadValue(RegisterType.MinPositionLimit);
					var max = servo.ReadValue(RegisterType.MaxPositionLimit);

					// ignore min and max for time being
					var value = 2048;

					writeRequests.Add(new WriteAsyncRequest
					{
						servo = servo
						,
						registerType = RegisterType.GoalPosition
					});
					servo.Registers[RegisterType.GoalPosition].Value = value;
				}
				PortPool.X.WriteAsync(writeRequests);
			}

			return new { };
		}
	}
}