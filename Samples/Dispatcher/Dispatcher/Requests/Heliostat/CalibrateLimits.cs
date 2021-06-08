using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

		public class Setting
		{

			public int min;
			public int max;
			public int range;
			public int mid;

			public Setting(int min, int max)
			{
				this.min = min;
				this.max = max;
				this.range = max - min;
				this.mid = (max + min) / 2;
			}
		}
		public struct Result
		{
			public Setting prior;
			public Setting post;
		}

		public int CurrentLimitForCalibration = 100;
		public int CurrentLimitAfterCalibration = 800;
		public int Velocity = 10;
		public double Timeout = 60;
		public int EndOffset = 4096 * 2 / 360; // 2 degrees
		public double Axis1AngleWhenCalibratingAxis2 = -90;
		public int PositionEpsilon = 20; // we don't need to be accurate here
		public int ProfileVelocity = 10;
		public int ProfileAcceleration = 1;
		public bool PrintToConsole = true;
		public bool DryRun = true;
		public int MinServoIndex = -1;
		public int MaxServoIndex = -1;
		public bool DoParallel = false;
		static object lockPort = new object ();

		private int readValueSync(DynamixelSDKSharp.Servo servo, RegisterType registerType)
		{
			lock(lockPort)
			{
				return servo.ReadValue(registerType);
			}
		}

		private void writeValueSync(DynamixelSDKSharp.Servo servo, RegisterType registerType, int value)
		{
			lock (lockPort)
			{
				servo.WriteValue(registerType, value);
			}
		}

		private int walkToLimit(DynamixelSDKSharp.Servo servo, int velocity)
		{
			if(this.PrintToConsole)
			{
				Console.WriteLine("Walking {0} to limit with velocity {1}", servo.ID, velocity);
			}

			// Torque off
			writeValueSync(servo, RegisterType.TorqueEnable, 0);

			// Take prior current limit and check if it looks OK
			var priorCurrentLimit = readValueSync(servo, RegisterType.CurrentLimit);

			// Set current limit for calibration
			writeValueSync(servo, RegisterType.CurrentLimit, this.CurrentLimitForCalibration);

			// Change into velocity control mode
			writeValueSync(servo, RegisterType.OperatingMode, 1);

			// Torque on
			writeValueSync(servo, RegisterType.TorqueEnable, 1);

			// Walk
			writeValueSync(servo, RegisterType.GoalVelocity, velocity);

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

				var presentCurrent = readValueSync(servo, RegisterType.PresentCurrent);
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
			var limitPosition = readValueSync(servo, RegisterType.PresentPosition);

			// Torque off (if it's still on)
			writeValueSync(servo, RegisterType.TorqueEnable, 0);

			// Set the mode back to position control
			writeValueSync(servo, RegisterType.OperatingMode, 3);

			// Set the current limit back to runtime
			writeValueSync(servo, RegisterType.CurrentLimit, this.CurrentLimitAfterCalibration);

			// Reboot
			lock(lockPort)
			{
				servo.Reboot();
			}

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

			writeValueSync(servo, RegisterType.TorqueEnable, 0);
			writeValueSync(servo, RegisterType.OperatingMode, 3);
			writeValueSync(servo, RegisterType.TorqueEnable, 1);
			writeValueSync(servo, RegisterType.ProfileAcceleration, this.ProfileAcceleration);
			writeValueSync(servo, RegisterType.ProfileVelocity, this.ProfileVelocity);
			writeValueSync(servo, RegisterType.GoalPosition, goalPosition);

			// Wait until we reach the target
			bool reachedEnd = false;
			var timeStart = DateTime.Now;
			while (!reachedEnd)
			{
				if (DateTime.Now - timeStart > TimeSpan.FromSeconds(this.Timeout))
				{
					throw (new Exception(String.Format("Timed out walking axis ({0}) to position ({1}). PresentPosition={2}", servo.ID, goalPosition, readValueSync(servo, RegisterType.PresentPosition))));
				}
				Thread.Sleep(100);

				// Check if we have reached the target position
				var presentPosition = readValueSync(servo, RegisterType.PresentPosition);
				if (Math.Abs(presentPosition - goalPosition) <= this.PositionEpsilon)
				{
					reachedEnd = true;
				}
			}
		}

		public object Perform()
		{
			// Don't print if parallel
			if (this.DoParallel)
			{
				this.PrintToConsole = false;
			}

			var servos = PortPool.X.Servos;

			// We don't look at the heliostats register, we just go with odd+even pairs = one heliostat
			var heliostats = new List<Heliostat>();

			foreach(var axis1ServoIndex in servos.Keys)
			{
				// Ignore if we set a maximum or minimum servo index on the request
				if(this.MinServoIndex >= 0 && axis1ServoIndex < this.MinServoIndex)
				{
					continue;
				}
				if(this.MaxServoIndex >= 0 && axis1ServoIndex > this.MaxServoIndex)
				{
					continue;
				}

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

			Action<Heliostat> processHeliostat = (Heliostat heliostat) =>
			{
				// Reboot at start
				{
					if (this.PrintToConsole)
					{
						Console.WriteLine("Rebooting {0} and {1}", heliostat.axis1.ID, heliostat.axis2.ID);
					}

					lock(lockPort)
					{
						heliostat.axis1.Reboot();
						heliostat.axis2.Reboot();
					}
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

					if (max - min < 2 * this.EndOffset)
					{
						throw (new Exception("Not enough space between max and min"));
					}

					var priorMin = readValueSync(heliostat.axis1, RegisterType.MinPositionLimit);
					var priorMax = readValueSync(heliostat.axis1, RegisterType.MaxPositionLimit);

					if (!this.DryRun)
					{
						writeValueSync(heliostat.axis1, RegisterType.MaxPositionLimit, max - this.EndOffset);
						writeValueSync(heliostat.axis1, RegisterType.MinPositionLimit, min + this.EndOffset);
					}

					axis1Center = (max + min) / 2;

					heliostatResult.Add(heliostat.axis1.ID, new Result
					{
						prior = new Setting(priorMin, priorMax)
						,
						post = new Setting(min, max)
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

					var priorMin = readValueSync(heliostat.axis2, RegisterType.MinPositionLimit);
					var priorMax = readValueSync(heliostat.axis2, RegisterType.MaxPositionLimit);

					if (!this.DryRun)
					{
						writeValueSync(heliostat.axis2, RegisterType.MaxPositionLimit, max - this.EndOffset);
						writeValueSync(heliostat.axis2, RegisterType.MinPositionLimit, min + this.EndOffset);
					}


					axis2Center = (min + max) / 2;

					heliostatResult.Add(heliostat.axis2.ID, new Result
					{
						prior = new Setting(priorMin, priorMax)
						,
						post = new Setting(min, max)
					});
				}

				// Walk axes back to center
				{
					// Walk 2 to min limit before starting
					walkAxis(heliostat.axis2, readValueSync(heliostat.axis2, RegisterType.MinPositionLimit));

					writeValueSync(heliostat.axis2, RegisterType.ProfileAcceleration, this.ProfileAcceleration);
					writeValueSync(heliostat.axis2, RegisterType.ProfileVelocity, this.ProfileVelocity);
					writeValueSync(heliostat.axis2, RegisterType.TorqueEnable, 1);
					writeValueSync(heliostat.axis2, RegisterType.GoalPosition, axis2Center);

					writeValueSync(heliostat.axis1, RegisterType.ProfileAcceleration, this.ProfileAcceleration);
					writeValueSync(heliostat.axis1, RegisterType.ProfileVelocity, this.ProfileVelocity);
					writeValueSync(heliostat.axis1, RegisterType.TorqueEnable, 1);
					writeValueSync(heliostat.axis1, RegisterType.GoalPosition, axis1Center);
				}

				results.Add(heliostatResult);
			};

			if(this.DoParallel)
			{
				Parallel.ForEach(heliostats, heliostat => processHeliostat(heliostat));
			}
			else
			{
				foreach(var heliostat in heliostats)
				{
					processHeliostat(heliostat);
				}
			}

			return results;
		}
	}
}