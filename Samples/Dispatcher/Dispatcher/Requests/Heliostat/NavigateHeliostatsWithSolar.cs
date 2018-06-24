using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Newtonsoft.Json;

using DynamixelSDKSharp;
using MongoDB.Driver;

namespace Dispatcher.Requests.Heliostat
{
    [RequestHandler(Method = Method.GET | Method.POST)]
    [Serializable]
    class NavigateHeliostatsWithSolar : IRequest
    {
		public double targetX { get; set; } = 0;
		public double targetY { get; set; } = 0;
		public double targetZ { get; set; } = 0;

		public bool useCustomTarget { get; set; } = false;

		public bool useSameTargetsAsLastTime { get; set; } = true;

        private static string hamNavigateURL = Program.HAMBaseURL + "navigateSunToPoint";
        private static string targetCSVPath = "C:\\Users\\elliot\\Dropbox (Kimchi and Chips)\\KC31 - Halo v2.0 Technical\\Target Plotting\\targets_live.csv";

		private static Dictionary<int, Models.Vector3> lastTargets = null;

		IEnumerable<Models.Heliostat> GetHeliostats()
		{
			return Program.Heliostats.Where(h => h.ID >= 0 &&
									h.ID <= 100 &&
									h.axis1Servo != null &&
									h.axis2Servo != null)
									.ToList();
		}

		Database.Models.ManualCalibration GetCalibration(int heliostatID, IMongoCollection<Database.Models.ManualCalibration> collection)
		{
			return collection.AsQueryable()
						.Where(doc => doc.HeliostatID == heliostatID)
						.OrderByDescending(doc => doc.TimeStamp)
						.Take(1)
						.First();
		}

        public object Perform()
        {
            var targets = new Dictionary<int, Models.Vector3>();

			//HACK
			var manualCalibrationPerHeliostat = Database.Models.ManualCalibration.GetLatestPerHeliostat();

			var heliostats = this.GetHeliostats();

			if (useSameTargetsAsLastTime && NavigateHeliostatsWithSolar.lastTargets != null)
			{
				targets = NavigateHeliostatsWithSolar.lastTargets;
			}
			else
			{
				if (this.useCustomTarget)
				{
					foreach (var heliostat in heliostats)
					{
						targets.Add(heliostat.ID, new Models.Vector3((float)this.targetX, (float)this.targetY, (float)this.targetZ));
					}
				}
				else
				{
					using (StreamReader file = new StreamReader(targetCSVPath))
					{
						file.ReadLine(); //Skip header
						string line;
						while ((line = file.ReadLine()) != null)
						{
							var row = line.Split(',');
							var heliostatID = Int32.Parse(row[0]);
							var x = (float)Double.Parse(row[4]) + (float) this.targetX;
							var y = (float)Double.Parse(row[5]) + (float) this.targetY;
							var z = (float)Double.Parse(row[6]) + (float) this.targetZ;

							var xyz = new Models.Vector3(x, y, z);

							targets.Add(heliostatID, xyz);
						}
					}
				}

				NavigateHeliostatsWithSolar.lastTargets = targets;
			}
			
            foreach (var heliostat in heliostats)
            {
                if (heliostat.axis1Servo == null || heliostat.axis2Servo == null) continue;

                var hamObject = new Models.HeliostatHAMPointToPointRequest();

				hamObject.hamParameters = heliostat.hamParameters;
				hamObject.targetPoint = targets[heliostat.ID];

                var json = JsonConvert.SerializeObject(hamObject);

                var HTTPClient = new HttpClient();

                try
                {
                    var requestContent = new StringContent(json);
                    var result = HTTPClient.PostAsync(hamNavigateURL, requestContent).Result;
                    result.EnsureSuccessStatusCode();

                    var response = result.Content.ReadAsStringAsync().Result;

                    var rotPitch = JsonConvert.DeserializeObject<Models.HeliostatHAMNavigateResponse>(response);

					//HACK
					int deltaA1, deltaA2;
					if(manualCalibrationPerHeliostat.ContainsKey(heliostat.ID))
					{
						var calibrationDoc = manualCalibrationPerHeliostat[heliostat.ID];
						deltaA1 = (int) (2048 - calibrationDoc.axis1ServoRegisters[RegisterType.PresentPosition.ToString()]);
						deltaA2 = (int) ((2048 - calibrationDoc.axis2ServoRegisters[RegisterType.PresentPosition.ToString()]) - (calibrationDoc.InclinometerValue * 4096 / 360));
					}
					else
					{
						deltaA1 = 0;
						deltaA2 = 0;
					}

					if(!Safety.ServoRestManager.X.IsResting(heliostat.axis1Servo))
					{
						heliostat.axis1Servo.WriteValue(RegisterType.GoalPosition
							, rotPitch.rotation - deltaA1
							, false);
					}


					if (!Safety.ServoRestManager.X.IsResting(heliostat.axis2Servo))
					{
						heliostat.axis2Servo.WriteValue(RegisterType.GoalPosition
							, rotPitch.pitch - deltaA2
							, false);
					}
                }
                catch (Exception ex)
                {
                    //throw new Exception("Couldn't navigate point to point: " + ex.Message);
                }
            }

            return new { };
        }
    }
}
