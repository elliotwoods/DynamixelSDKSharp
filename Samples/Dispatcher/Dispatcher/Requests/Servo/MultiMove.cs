﻿using DynamixelSDKSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("movements = [{movements.Length}], waitUntilComplete = {waitUntilComplete}")]
	class MultiMove : IRequest
	{
		public class Movement
		{
			public int servoID;
			public int position;
		}

		public List<Movement> movements = new List<Movement>();

		public bool waitUntilComplete = false;

		public int epsilon = 1;

		public double timeout = 20.0f;

		public object Perform()
		{
			// Store the servos for readback
			var servos = new List<DynamixelSDKSharp.Servo>();

			var writeAsyncRequests = new List<WriteAsyncRequest>();

			// Get the servos, store them, and send goal positions
			{
				foreach(var movement in this.movements)
				{
					var servo = PortPool.X.FindServo(movement.servoID);

					// check if the value is in the valid range
					if(movement.position < 0 || movement.position > servo.ProductSpecification.EncoderResolution)
					{
						throw (new Exception(String.Format("Position {0} out of range for servo {1}", movement.position, movement.servoID)));
					}

					// Write it to the local servo
					servo.Registers[RegisterType.GoalPosition].Value = movement.position;

					// Add a request to push this variable
					var writeAsyncRequest = new WriteAsyncRequest
					{
						servo = servo
						, registerType = RegisterType.GoalPosition
					};
					writeAsyncRequests.Add(writeAsyncRequest);

					servos.Add(servo);
				}
			}

			PortPool.X.WriteAsync(writeAsyncRequests);

			// Wait until complete
			if (this.waitUntilComplete)
			{
				var timeStart = DateTime.Now;
				var servosStillWaiting = new List<DynamixelSDKSharp.Servo>(servos);

				while (servosStillWaiting.Count > 0)
				{
					servosStillWaiting.RemoveAll(servo =>
					{
						return Math.Abs(servo.ReadValue(RegisterType.PresentPosition) - servo.ReadValue(RegisterType.GoalPosition)) <= this.epsilon;
					});

					if (DateTime.Now - timeStart > TimeSpan.FromSeconds(this.timeout))
					{
						var servoNames = servos.Select(servo =>
						{
							return servo.ID.ToString();
						});

						throw (new Exception("Timeout moving to goal position for servos " + String.Join(", ", servoNames)));
					}

					Thread.Sleep(1);
				}
			}

			return new { };
		}
	}
}