using Dispatcher.Database;
using DynamixelSDKSharp;
using MongoDB.Driver;
using Newtonsoft.Json;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Requests.Heliostat
{
	class NavigateToSolar : IRequest
	{
		private static string hamPointToPointURL = "http://localhost:8080/navigatePointToPoint";

		public object HTTPClient { get; private set; }

		public object Perform()
		{
			//get the last and next solar vector from MongoDB
			var collection = Database.Connection.X.GetCollection<Database.Models.SolarVector>();

			var now = DateTime.Now;

			Database.Models.SolarVector solarVectorBefore = null;
			Database.Models.SolarVector solarVectorAfter = null;

			var solarVectors = new Dictionary<DateTime, Vector3D>();

			try
			{
				solarVectorBefore = collection.AsQueryable()
					.Where(doc => doc.time <= now)
					.OrderBy(doc => doc.time)
					.First();
				solarVectors.Add(solarVectorBefore.time, solarVectorBefore.vector);
			}
			catch
			{

			}

			try
			{
				solarVectorAfter = collection.AsQueryable()
					.Where(doc => doc.time > now)
					.OrderByDescending(doc => doc.time)
					.First();
				solarVectors.Add(solarVectorBefore.time, solarVectorBefore.vector);
			}
			catch
			{

			}

			var solarVector = new Vector3D();
			if(solarVectors.Count == 0)
			{
				throw (new Exception("No solar data in database"));
			}
			else if(solarVectors.Count == 1)
			{
				solarVector = solarVectors.First().Value;
			}
			else
			{
				//interpolate between the 2
				var distanceTo1 = Math.Abs((now - solarVectors.First().Key).TotalMilliseconds);
				var distanceTo2 = Math.Abs((now - solarVectors.Last().Key).TotalMilliseconds);

				var firstValue = solarVectors.First().Value;
				var lastValue = solarVectors.Last().Value;

				var ratioAlongInterpolation = distanceTo1 / (distanceTo1 + distanceTo2);
				solarVector = lastValue * ratioAlongInterpolation + firstValue * (1.0f - ratioAlongInterpolation);
			}


			var heliostats = Program.Heliostats;
			var HTTPClient = new HttpClient();

			//We should be using a Task.WhenAll here to make the requests async in parallel
			foreach (var heliostat in heliostats)
			{
				//take the current register (maybe just take local cache)
				var a1Before = heliostat.axis1Servo.Registers[RegisterType.GoalPosition].Value;
				var a2Before = heliostat.axis1Servo.Registers[RegisterType.GoalPosition].Value;

				//feed the vector and params into HAM request
				Models.HeliostatHAMNavigateResponse servoValues;
				try
				{
					var requestObject = new Models.HeliostatHAMVectorToPointRequest
					{
						hamParameters = heliostat.hamParameters,
						currentServoSetting = new Models.HeliostatHamRequestCurrentPosition
						{	
							rotation = a1Before,
							pitch = a2Before
						}
					};

					var requestContent = new StringContent(JsonConvert.SerializeObject(requestObject));
					var result = HTTPClient.PostAsync(hamPointToPointURL, requestContent).Result;
					result.EnsureSuccessStatusCode();

					var response = result.Content.ReadAsStringAsync().Result;

					servoValues = JsonConvert.DeserializeObject<Models.HeliostatHAMNavigateResponse>(response);
				}
				catch (Exception ex)
				{
					throw new Exception("Couldn't navigate : " + ex.Message);
				}

				//write the values to servos
				heliostat.axis1Servo.WriteValue(RegisterType.GoalPosition, servoValues.rotation);
				heliostat.axis2Servo.WriteValue(RegisterType.GoalPosition, servoValues.pitch);
			}

			return new { };
		}
	}
}
