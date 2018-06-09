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
	class RecordDatum : IRequest
	{
		public int heliostatID { get; set; }
		public float value { get; set; }
		public object Perform()
		{
			var collection = Database.Connection.X.GetCollection<Database.Registers>();

			var heliostat = Program.Heliostats.Where(h => h.ID == heliostatID).SingleOrDefault();
			if (heliostat == null)
			{
				throw new Exception(String.Format("Heliostat with ID {0} does not exist.", heliostatID));
			}

			

			var valuesForServo = new Dictionary<string, int>();
			valuesForServo.Add("PresentPosition"
					, heliostat.axis1Servo.ReadValue(RegisterType.PresentPosition));
			valuesForServo.Add("Datum"
					, heliostat.axis1Servo.ReadValue(RegisterType.PresentPosition));
			var row = new Database.Registers
			{
				ServoID = heliostat.axis1ServoID,
				RegisterValues = valuesForServo
			};
			collection.InsertOne(row);

			valuesForServo = new Dictionary<string, int>();
			var PresentPosition = heliostat.axis2Servo.ReadValue(RegisterType.PresentPosition);
			var inclinometer = value;
			var CalculatedDatum = datumCalculator(PresentPosition, inclinometer);

			valuesForServo.Add("PresentPosition"
					, heliostat.axis2Servo.ReadValue(RegisterType.PresentPosition));
			valuesForServo.Add("InclinometerValue"
					, (int)value);
			valuesForServo.Add("Datum"
					, (int)CalculatedDatum);
			row = new Database.Registers
			{
				ServoID = heliostat.axis2ServoID,
				RegisterValues = valuesForServo
			};
			collection.InsertOne(row);

			return new { };

		}

		public float datumCalculator(int presentPosition, float incliometerValue)
		{
			var resultDatum = presentPosition + incliometerValue * 4096 / 360;
			return resultDatum;
		}

	}

	
}
