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

		public int CurrentLimit = 100;
		public int Velocity = 10;
		public double Timeout = 30;
		public int EndOffset = 4096 * 5 / 360; // 5 degrees

		private int walkToLimit(DynamixelSDKSharp.Servo servo, int velocity)
		{
			// Torque off
			servo.WriteValue(RegisterType.TorqueEnable, 0);

			// Take prior current limit and check if it looks OK
			var priorCurrentLimit = servo.ReadValue(RegisterType.CurrentLimit);
			if (priorCurrentLimit <= this.CurrentLimit)
			{
				// We maybe failed in previous calibration
				throw (new Exception(String.Format("Servo {0} Prior current limit ({2}) <= Calibration current limit ({3})"
					, servo.ID
					, priorCurrentLimit
					, this.CurrentLimit)));
			}

			// Set current limit for calibration
			servo.WriteValue(RegisterType.CurrentLimit, this.CurrentLimit);

			// Change into velocity control mode
			servo.WriteValue(RegisterType.OperatingMode, 1);

			// Torque on
			servo.WriteValue(RegisterType.TorqueEnable, 1);

			// Walk
			servo.WriteValue(RegisterType.GoalVelocity, velocity);

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

				// Check if we have an overcurrent error
				var errors = servo.ReadValue(RegisterType.HardwareErrorStatus);
				if ((errors | 32) != 0)
				{
					reachedEnd = true;
				}
			}

			// Get the limit position
			var limitPosition = servo.ReadValue(RegisterType.PresentPosition);

			// Reboot
			servo.Reboot();

			return limitPosition;
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

			foreach(var heliostat in heliostats)
			{
				// Axis 1
				{
					var max = this.walkToLimit(heliostat.axis1, this.Velocity);
					var min = this.walkToLimit(heliostat.axis1, -this.Velocity);

					if(max - min < 2 * this.EndOffset)
					{
						throw (new Exception("Not enough space between max and min"));
					}

					heliostat.axis1.WriteValue(RegisterType.MaxPositionLimit, max - this.EndOffset);
					heliostat.axis1.WriteValue(RegisterType.MinPositionLimit, min + this.EndOffset);
				}
			}

			return new { };
		}
	}
}