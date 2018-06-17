using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;

namespace Dispatcher.Requests.Heliostat
{
	[RequestHandler(ThreadUsage = ThreadUsage.Exclusive)]
	[Serializable]
	public class Refresh : IRequest
	{
		private const string HelioStatsConfigurationJsonPath = "Heliostats.json";

		public object Perform()
		{
			//load the json file into a List
			using (StreamReader file = new StreamReader(HelioStatsConfigurationJsonPath))
			{
				var json = file.ReadToEnd();
				Program.Heliostats = JsonConvert.DeserializeObject<List<Models.Heliostat>>(json);
			}

			Console.WriteLine("Loaded {0} heliostats from {1}.", Program.Heliostats.Count, HelioStatsConfigurationJsonPath);

			//Check for Servos which are determined in teh heliostats file but we haven't find on any serial ports
            var servoHeliostatMismatchLog = new List<string>();
			foreach (var h in Program.Heliostats)
			{
				try
				{
					h.axis1Servo = PortPool.X.Servos[h.axis1ServoID];
				}
				catch (Exception ex)
				{
                    servoHeliostatMismatchLog.Add(String.Format("Couldn't find axis 1 servo ({0}) for heliostat {1}", h.axis1ServoID, h.ID));
				}

				try
				{
					h.axis2Servo = PortPool.X.Servos[h.axis2ServoID];
				}
				catch (Exception ex)
				{
					servoHeliostatMismatchLog.Add(String.Format("Couldn't find axis 2 servo ({0}) for heliostat {1}", h.axis2ServoID, h.ID));
				}
			}

            if (servoHeliostatMismatchLog.Count > 0)
            {
                var s = "";
                foreach (var mm in servoHeliostatMismatchLog)
                {
                    s += mm + "\n";
                }
                throw new Exception("Servo/Heliostat mismatches:\n" + s);
            }

			return new {
				heliostatCount = Program.Heliostats.Count
			};

		}
	}
}
