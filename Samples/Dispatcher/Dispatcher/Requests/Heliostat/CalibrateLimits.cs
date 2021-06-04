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
					var axis = heliostat.axis1;

					// Torque off
					axis.WriteValue(RegisterType.TorqueEnable, 0);

					// Take prior current limit and check if it looks OK
					var priorCurrentLimit = axis.ReadValue(RegisterType.CurrentLimit);
					if(priorCurrentLimit <= this.CurrentLimit)
					{
						// We maybe failed in previous calibration
						throw (new Exception(String.Format("Servo {0} Prior current limit ({2}) <= Calibration current limit ({3})"
							, axis.ID
							, priorCurrentLimit
							, this.CurrentLimit)));
					}

					// Set current limit for calibration
					axis.WriteValue(RegisterType.CurrentLimit, this.CurrentLimit);

					// Change into velocity control mode
					axis.WriteValue(RegisterType.OperatingMode, 1);

					// Torque on
					axis.WriteValue(RegisterType.TorqueEnable, 1);

					// Walk forwards
					axis.WriteValue(RegisterType.GoalVelocity, this.Velocity);

					// Wait until we reach the end
					bool reachedEnd = false;
					var timeStart = DateTime.Now;
					while(!reachedEnd)
					{
						if(DateTime.Now - timeStart > TimeSpan.FromSeconds(this.Timeout))
						{
							throw (new Exception("Timed out moving to axis limit"));
						}
						Thread.Sleep(100);

						// Check if we have an overcurrent error
						var errors = axis.ReadValue(RegisterType.HardwareErrorStatus);
						if((errors | 32) != 0)
						{
							reachedEnd = true;
						}
					}

					// Reset

				}
			}

			return new { };
		}
	}
}