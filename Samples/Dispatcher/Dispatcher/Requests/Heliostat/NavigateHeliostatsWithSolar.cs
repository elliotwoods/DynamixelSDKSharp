using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Newtonsoft.Json;

using DynamixelSDKSharp;

namespace Dispatcher.Requests.Heliostat
{
    [RequestHandler(Method = Method.GET)]
    [Serializable]
    class NavigateHeliostatsWithSolar : IRequest
    {
        private static string hamNavigateURL = Program.HAMBaseURL + "navigateSunToPoint";
        private static string targetCSVPath = "targets.csv";

        public object Perform()
        {
            var targets = new Dictionary<int, Models.Vector3>();

            using (StreamReader file = new StreamReader(targetCSVPath))
            {
                file.ReadLine(); //Skip header
                string line;
                while ((line = file.ReadLine()) != null) {
                    var hstat = Int32.Parse(line.Split(',')[0]);
                    var x = (float)Double.Parse(line.Split(',')[4]);
                    var y = (float)Double.Parse(line.Split(',')[5]);
                    var z = (float)Double.Parse(line.Split(',')[6]);
                    /*var x = 5.0f;
                    var y = 2.0f;
                    var z = 4.0f;*/

                    var v = new Models.Vector3(x, y, z);

                    targets.Add(hstat, v);
                }
            }

            var heliostats = Program.Heliostats.Where(h => h.ID >= 0 &&
                                    h.ID <= 100 &&
                                    h.axis1Servo != null &&
                                    h.axis2Servo != null)
                                    .ToList();

            foreach (var h in heliostats)
            {
                if (h.axis1Servo == null || h.axis2Servo == null) continue;

                var hamObject = new Models.HeliostatHAMPointToPointRequest();
                hamObject.targetPoint = targets[h.ID];
                hamObject.hamParameters = h.hamParameters;

                var json = JsonConvert.SerializeObject(hamObject);

                var HTTPClient = new HttpClient();

                try
                {
                    var requestContent = new StringContent(json);
                    var result = HTTPClient.PostAsync(hamNavigateURL, requestContent).Result;
                    result.EnsureSuccessStatusCode();

                    var response = result.Content.ReadAsStringAsync().Result;

                    var rotPitch = JsonConvert.DeserializeObject<Models.HeliostatHAMNavigateResponse>(response);

                    h.axis1Servo.WriteValue(RegisterType.GoalPosition, rotPitch.rotation);
                    h.axis2Servo.WriteValue(RegisterType.GoalPosition, rotPitch.pitch);
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
