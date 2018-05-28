using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Database
{
	class HighFrequencyRequests
	{
		public static HighFrequencyRequests X = new HighFrequencyRequests();

		Dictionary<int, DateTime> FExpirationPerServo = new Dictionary<int, DateTime>();
		object FExpirationPerServoLock = new object();

		class Request
		{
			public int ServoID { get; set; }
			public DateTime Expiration { get; set; }
		}

		HighFrequencyRequests()
		{

		}

		public void AddRequest(int servoID, TimeSpan duration)
		{
			lock(this.FExpirationPerServoLock)
			{
				var expiration = DateTime.Now + duration;
				if (this.FExpirationPerServo.ContainsKey(servoID))
				{
					this.FExpirationPerServo[servoID] = expiration;
				}
				else
				{
					this.FExpirationPerServo.Add(servoID, expiration);
				}
			}
		}

		public bool IsHighFrequencyNow(int servoID)
		{
			lock(this.FExpirationPerServoLock)
			{
				//flush expired entries
				{
					var servoIDsToRemove = this.FExpirationPerServo.Where(pair => pair.Value < DateTime.Now)
					.Select(pair => pair.Key)
					.ToList();
					foreach (var servoIDToRemove in servoIDsToRemove)
					{
						this.FExpirationPerServo.Remove(servoIDToRemove);
					}
				}

				return this.FExpirationPerServo.ContainsKey(servoID);
			}
		}
	}
}
