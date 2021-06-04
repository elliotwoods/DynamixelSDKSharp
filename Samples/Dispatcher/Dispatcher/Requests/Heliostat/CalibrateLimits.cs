using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Dispatcher.Requests.Heliostat
{

	[RequestHandler(Method = Method.POST)]
	[Serializable]
	class CalibrateLimits : IRequest
	{
		private struct Heliostat
		{
			public DynamixelSDKSharp.Servo axis1;
			public DynamixelSDKSharp.Servo axis2;
		}

		public struct Result
		{
			public int min;
			public int max;
			public int range;
		}

		public int CurrentLimitForCalibration = 100;
		public int CurrentLimitAfterCalibration = 1941;
		public int Velocity = 10;
		public double Timeout = 60;
		public int EndOffset = 4096 * 5 / 360; // 5 degrees
		public double Axis1AngleWhenCalibratingAxis2 = -90;
		public int PositionEpsilon = 10; // we don't need to be accurate here
		public int ProfileVelocity = 10;
		public int ProfileAcceleration = 1;
		public bool PrintToConsole = true;

		private int walkToLimit(DynamixelSDKSharp.Servo servo, int velocity)
		{
			if(this.PrintToConsole)
			{
				Console.WriteLine("Walking {0} to limit with velocity {1}", servo.ID, velocity);
			}

			// Torque off
			servo.WriteValue(RegisterType.TorqueEnable, 0);

			// Take prior current limit and check if it looks OK
			var priorCurrentLimit = servo.ReadValue(RegisterType.CurrentLimit);

			// Set current limit for calibration
			servo.WriteValue(RegisterType.CurrentLimit, this.CurrentLimitForCalibration);

			// Change into velocity control mode
			servo.WriteValue(RegisterType.OperatingMode, 1);

			// Torque on
			servo.WriteValue(RegisterType.TorqueEnable, 1);

			// Walk
			servo.WriteValue(RegisterType.GoalVelocity, velocity);

			// Give it a second to get started (lots of torque when jammed)
			Thread.Sleep(2000);

			// Wait until we reach the end
			bool reachedEnd = false;
			var timeStart = DateTime.Now;
			while (!reachedEnd)
			{
				if (DateTime.Now - timeStart > TimeSpan.FromSeconds(this.Timeout))
				{
					throw (new Exception(String.Format("Timed out moving to axis limit on Servo ({0})", servo.ID)));
				}
				Thread.Sleep(100);

				var presentCurrent = servo.ReadValue(RegisterType.PresentCurrent);
				if(presentCurrent > 1 << 15)
				{
					presentCurrent = (1 << 16) - presentCurrent;
				}
				if (Math.Abs(presentCurrent) > this.CurrentLimitForCalibration)
				{
					reachedEnd = true;
				}
			}

			// Get the limit position
			var limitPosition = servo.ReadValue(RegisterType.PresentPosition);

			// Torque off (if it's still on)
			servo.WriteValue(RegisterType.TorqueEnable, 0);

			// Set the mode back to position control
			servo.WriteValue(RegisterType.OperatingMode, 3);

			// Set the current limit back to runtime
			servo.WriteValue(RegisterType.CurrentLimit, this.CurrentLimitAfterCalibration);

			// Reboot
			servo.Reboot();

			// Wait for reboot
			Thread.Sleep(3000);

			return limitPosition;
		}

		void walkAxis(DynamixelSDKSharp.Servo servo, int goalPosition)
		{
			if (this.PrintToConsole)
			{
				Console.WriteLine("Walking {0} to position {1}", servo.ID, goalPosition);
			}

			servo.WriteValue(RegisterType.TorqueEnable, 0);
			servo.WriteValue(RegisterType.OperatingMode, 3);
			servo.WriteValue(RegisterType.TorqueEnable, 1);
			servo.WriteValue(RegisterType.ProfileAcceleration, this.ProfileAcceleration);
			servo.WriteValue(RegisterType.ProfileVelocity, this.ProfileVelocity);
			servo.WriteValue(RegisterType.GoalPosition, goalPosition);

			// Wait until we reach the target
			bool reachedEnd = false;
			var timeStart = DateTime.Now;
			while (!reachedEnd)
			{
				if (DateTime.Now - timeStart > TimeSpan.FromSeconds(this.Timeout))
				{
					throw (new Exception(String.Format("Timed out walking axis ({0}) to position ({1})", servo.ID, goalPosition)));
				}
				Thread.Sleep(100);

				// Check if we have reached the target position
				var presentPosition = servo.ReadValue(RegisterType.PresentPosition);
				if (Math.Abs(presentPosition - goalPosition) <= this.PositionEpsilon)
				{
					reachedEnd = true;
				}
			}
		}

		public object Perform()
		{
			var servos = PortPool.X.Servos;

			// We don't look at the heliostats register, we just go with odd+even pairs = one heliostat
			var heliostats = new List<Heliostat>();

			foreach(var axis1ServoIndex in servos.Keys)
			{
				if(axis1ServoIndex % 2 == 1)
				{
					// We have an odd index, check if matching even index exists
					if (servos.Keys.Contains(axis1ServoIndex + 1)) {
						heliostats.Add(new Heliostat
						{
							axis1 = servos[axis1ServoIndex]
							, axis2 = servos[axis1ServoIndex + 1]
						});
					}
				}
			}

			var results = new List<Dictionary<int, Result>>();

			foreach(var heliostat in heliostats)
			{
				// Reboot at start
				{
					if (this.PrintToConsole)
					{
						Console.WriteLine("Rebooting {0} and {1}", heliostat.axis1.ID, heliostat.axis2.ID);
					}

					heliostat.axis1.Reboot();
					heliostat.axis2.Reboot();
					Thread.Sleep(3000);
				}

				// Walk axis 2 to position to calibrate axis 1
				{
					walkAxis(heliostat.axis2, 2048);
				}

				var heliostatResult = new Dictionary<int, Result>();

				// Axis 1
				int axis1Center;
				{
					var max = this.walkToLimit(heliostat.axis1, this.Velocity);
					var min = this.walkToLimit(heliostat.axis1, -this.Velocity);

					if(max - min < 2 * this.EndOffset)
					{
						throw (new Exception("Not enough space between max and min"));
					}

					heliostat.axis1.WriteValue(RegisterType.MaxPositionLimit, max - this.EndOffset);
					heliostat.axis1.WriteValue(RegisterType.MinPositionLimit, min + this.EndOffset);
					axis1Center = (max + min) / 2;

					heliostatResult.Add(heliostat.axis1.ID, new Result
					{
						min = min
						, max = max
						, range = max - min
					});
				}

				// Walk axis 1 to position to calibrate axis 2
				{
					var goalPosition = (int)(this.Axis1AngleWhenCalibratingAxis2 / 360.0 * 4096.0) + axis1Center;
					walkAxis(heliostat.axis1, goalPosition);
				}

				// Axis 2
				int axis2Center;
				{
					var max = this.walkToLimit(heliostat.axis2, this.Velocity);
					var min = this.walkToLimit(heliostat.axis2, -this.Velocity);

					if (max - min < 2 * this.EndOffset)
					{
						throw (new Exception("Not enough space between max and min"));
					}

					heliostat.axis2.WriteValue(RegisterType.MaxPositionLimit, max - this.EndOffset);
					heliostat.axis2.WriteValue(RegisterType.MinPositionLimit, min + this.EndOffset);

					axis2Center = (min + max) / 2;

					heliostatResult.Add(heliostat.axis2.ID, new Result
					{
						min = min
						, max = max
						, range = max - min
					});
				}

				// Walk axes back to center
				{
					// Walk 2 to min limit before starting
					walkAxis(heliostat.axis2, heliostat.axis2.ReadValue(RegisterType.MinPositionLimit));

					heliostat.axis2.WriteValue(RegisterType.ProfileAcceleration, this.ProfileAcceleration);
					heliostat.axis2.WriteValue(RegisterType.ProfileVelocity, this.ProfileVelocity);
					heliostat.axis2.WriteValue(RegisterType.TorqueEnable, 1);
					heliostat.axis2.WriteValue(RegisterType.GoalPosition, axis2Center);

					heliostat.axis1.WriteValue(RegisterType.ProfileAcceleration, this.ProfileAcceleration);
					heliostat.axis1.WriteValue(RegisterType.ProfileVelocity, this.ProfileVelocity);
					heliostat.axis1.WriteValue(RegisterType.TorqueEnable, 1);
					heliostat.axis1.WriteValue(RegisterType.GoalPosition, axis1Center);
				}

				results.Add(heliostatResult);
			}

			return results;
		}
	}
}