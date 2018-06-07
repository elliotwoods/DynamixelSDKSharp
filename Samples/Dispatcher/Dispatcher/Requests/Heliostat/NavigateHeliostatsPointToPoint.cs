using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;

using DynamixelSDKSharp;

namespace Dispatcher.Requests.Heliostat
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("Heliostat = {heliostatID}, targetPoint = {targetPoint}, source = {source}")]
	class NavigateHeliostatsPointToPoint : IRequest
	{
		private static string hamPointToPointURL = "http://10.0.0.27:8080/navigatePointToPoint";

		public int startHeliostatID { get; set; }
		public int endHelioStatID { get; set; }

		public Models.Vector3 targetPoint { get; set; }
		public Models.Vector3 source { get; set; }

		public object Perform()
		{
			var heliostats = Program.Heliostats.Where(h => h.ID >= startHeliostatID &&
									h.ID <= endHelioStatID && 
									h.axis1Servo != null && 
									h.axis2Servo != null)
									.ToList();

			foreach (var h in heliostats)
			{
				if (h.axis1Servo == null || h.axis2Servo == null) continue;

				var hamObject = new Models.HeliostatHAMPointToPointRequest();
				hamObject.source = source;
				hamObject.targetPoint = targetPoint;
				hamObject.hamParameters = h.hamParameters;

				var json = JsonConvert.SerializeObject(hamObject);

				var HTTPClient = new HttpClient();

				try
				{
					var requestContent = new StringContent(json);
					var result = HTTPClient.PostAsync(hamPointToPointURL, requestContent).Result;
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
