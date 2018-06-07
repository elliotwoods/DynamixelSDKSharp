using System;
using System.Diagnostics;
using System.Net.Http;
using System.Linq;

using Newtonsoft.Json;

using DynamixelSDKSharp;

namespace Dispatcher.Requests.Heliostat
{
	
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("Heliostat = {heliostatID}, targetPoint = {targetPoint}, source = {source}")]
	public class NavigatePointToPoint : IRequest
	{
		private static string hamPointToPointURL = "http://localhost:8080/navigatePointToPoint";

		public int heliostatID { get; set; }

		public Models.Vector3 targetPoint { get; set; }
		public Models.Vector3 source { get; set; }

		public object Perform()
		{
			var heliostat = Program.Heliostats.Where(h => h.ID == heliostatID).SingleOrDefault();
			if (heliostat == null)
			{
				throw new Exception(String.Format("Heliostat with ID {0} does not exist.", heliostatID));
			}

			var hamObject = new Models.HeliostatHAMPointToPointRequest();
			hamObject.source = source;
			hamObject.targetPoint = targetPoint;
			hamObject.hamParameters = heliostat.hamParameters;

			var json = JsonConvert.SerializeObject(hamObject);
			
			var HTTPClient = new HttpClient();

			try
			{
				var requestContent = new StringContent(json);
				var result = HTTPClient.PostAsync(hamPointToPointURL, requestContent).Result;
				result.EnsureSuccessStatusCode();

				var response = result.Content.ReadAsStringAsync().Result;

				var rotPitch = JsonConvert.DeserializeObject<Models.HeliostatHAMNavigateResponse>(response);

				heliostat.axis1Servo.WriteValue(RegisterType.GoalPosition, rotPitch.rotation);
				heliostat.axis2Servo.WriteValue(RegisterType.GoalPosition, rotPitch.pitch);

				return rotPitch;
			} catch (Exception ex)
			{
				throw new Exception("Couldn't navigate point to point: " + ex.Message);
			}			
		}
	}
}
