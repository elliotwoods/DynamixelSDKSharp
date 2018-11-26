using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelREST.Safety
{
	class ServoRestManager
	{
		//Singleton
		public static readonly ServoRestManager X = new ServoRestManager();

		public static TimeSpan RestingTime = TimeSpan.FromMinutes(5);

		class ServoRest
		{
			public DateTime RestTimeBegan = DateTime.Now;
			public Servo Servo;
		}

		List<ServoRest> ServoRests = new List<ServoRest>();

		private ServoRestManager()
		{

		}

		public void PutToRest(Servo servo)
		{
			servo.WriteValue(RegisterType.TorqueEnable, 0);

			//check if already exists
			var queryExisting = ServoRests.Where(rs => rs.Servo == servo);

			if(queryExisting.Any())
			{
				queryExisting.First().RestTimeBegan = DateTime.Now;
			}
			else
			{
				this.ServoRests.Add(new ServoRest
				{
					RestTimeBegan = DateTime.Now,
					Servo = servo
				});
			}
		}

		public bool IsResting(Servo servo)
		{
			var queryRestingServos = this.ServoRests.Where(rs => rs.Servo == servo);
			if(queryRestingServos.Any())
			{
				//we have a rest assigned for this servo

				//remove all rests which are stale
				this.ServoRests.RemoveAll(rs => DateTime.Now - rs.RestTimeBegan > ServoRestManager.RestingTime);

				//check if we still ahve one for this servo
				var newQueryRestingServos = this.ServoRests.Where(rs => rs.Servo == servo);

				if(newQueryRestingServos.Any()) {
					return true;
				}
				else
				{
					//wake it up
					servo.WriteValue(RegisterType.TorqueEnable, 1);

					//no longer resting
					return false;
				}
			}
			else
			{
				return false;
			}
		}
	}
}
