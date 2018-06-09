using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dispatcher.Requests.ManualCalibration
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	class Record : IRequest
	{
		public int heliostatID { get; set; }
		public double inclinometerValue { get; set; }

		public object Perform()
		{

			var heliostat = Program.Heliostats.Where(h => h.ID == heliostatID).SingleOrDefault();
			if (heliostat == null)
			{
				throw new Exception(String.Format("Heliostat with ID {0} does not exist.", heliostatID));
			}

			var registersToLog = new List<RegisterType>{
				RegisterType.PresentPosition,
				RegisterType.GoalPosition,
				RegisterType.PresentTemperature,
				RegisterType.PresentCurrent
			};

			//log to database
			var row = new Database.Models.ManualCalibration();
			{
				var collection = Database.Connection.X.GetCollection<Database.Models.ManualCalibration>();

				row.HeliostatID = this.heliostatID;

				foreach(var registerType in registersToLog)
				{
					row.axis1ServoRegisters.Add(registerType.ToString(), heliostat.axis1Servo.ReadValue(registerType));
					row.axis2ServoRegisters.Add(registerType.ToString(), heliostat.axis2Servo.ReadValue(registerType));
				}

				row.InclinometerValue = this.inclinometerValue;

				collection.InsertOne(row);
			}

			//perform correction on axis 2
			{
				var presentPosition = row.axis2ServoRegisters[RegisterType.PresentPosition.ToString()];
				var delta = (2048 - presentPosition) - (inclinometerValue * 4096 / 360);
				var goalPosition = 2048 - delta;

				heliostat.axis2Servo.WriteValue(RegisterType.GoalPosition, (int) goalPosition);
			}

			return new { };
		}
	}
}
