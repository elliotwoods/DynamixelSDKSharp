using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;


namespace Dispatcher.Requests.ManualCalibration
{
	[RequestHandler(Method = Method.GET)]
	[Serializable]
	class Prepare : IRequest
	{
		public object Perform()
		{
			var manualCalibrationPerHeliostat = Database.Models.ManualCalibration.GetLatestPerHeliostat();
			//Set registers for all heliostats
			foreach (var h in Program.Heliostats)
			{
				int deltaA1, deltaA2;
				if (manualCalibrationPerHeliostat.ContainsKey(h.ID))
				{
					var calibrationDoc = manualCalibrationPerHeliostat[h.ID];
					deltaA1 = 2048 - (int)(calibrationDoc.axis1ServoRegisters[RegisterType.PresentPosition.ToString()]);
					deltaA2 = (int)((2048 - calibrationDoc.axis2ServoRegisters[RegisterType.PresentPosition.ToString()]) - (calibrationDoc.InclinometerValue * 4096 / 360));
				}
				else
				{
					deltaA1 = 0;
					deltaA2 = 0;
				}


				try
				{
					h.axis1Servo = PortPool.X.Servos[h.axis1ServoID];
					h.axis1Servo.WriteValue(RegisterType.TorqueEnable, 0);
				}
				catch (Exception ex)
				{
					//throw new Exception(String.Format("Couldn't find axis 1 servo ({0}) for heliostat {1}", h.axis1ServoID, h.ID));
				}

				try
				{
					h.axis2Servo = PortPool.X.Servos[h.axis2ServoID];
					h.axis2Servo.WriteValue(RegisterType.TorqueEnable, 1);
					h.axis2Servo.WriteValue(RegisterType.ProfileVelocity, 10);
					h.axis2Servo.WriteValue(RegisterType.ProfileAcceleration, 1);
					h.axis2Servo.WriteValue(RegisterType.PositionIGain, 200);
					h.axis2Servo.WriteValue(RegisterType.GoalPosition, 2048 - deltaA2);
				}
				catch (Exception ex)
				{
					//throw new Exception(String.Format("Couldn't find axis 2 servo ({0}) for heliostat {1}", h.axis1ServoID, h.ID));
				}
			}

			//disable the scheduler
			Dispatcher.Scheduler.X.Enabled = false;
			return new { };
		}
	}
}
