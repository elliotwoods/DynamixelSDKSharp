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
			using (StreamReader file = new StreamReader(HelioStatsConfigurationJsonPath))
			{
				var json = file.ReadToEnd();
				Program.Heliostats = JsonConvert.DeserializeObject<List<Models.Heliostat>>(json);
			}

			Console.WriteLine("Loaded {0} heliostats from {1}.", Program.Heliostats.Count, HelioStatsConfigurationJsonPath);

			foreach (var h in Program.Heliostats)
			{
				try
				{
					h.axis1Servo = PortPool.X.Servos[h.axis1ServoID];
				} catch (Exception ex)
				{
					//throw new Exception(String.Format("Couldn't find axis 1 servo ({0}) for heliostat {1}", h.axis1ServoID, h.ID));
				}

				try
				{
					h.axis2Servo = PortPool.X.Servos[h.axis2ServoID];
				}
				catch (Exception ex)
				{
					//throw new Exception(String.Format("Couldn't find axis 2 servo ({0}) for heliostat {1}", h.axis1ServoID, h.ID));
				}
			}

			return new {
				heliostatCount = Program.Heliostats.Count
			};

		}
	}
}
