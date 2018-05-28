using DynamixelSDKSharp;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Requests.Data
{
	[Serializable]
	class Logger : IRequest
	{
		const string Filename = "DataLogger.json";

		class Settings
		{
			public Settings()
			{
				this.Load(Logger.Filename);
			}

			void Load(string filename)
			{
				//load list of constraints
				using (StreamReader file = new StreamReader(filename))
				{
					var json = file.ReadToEnd();
					JsonConvert.PopulateObject(json
						, this
						, ProductDatabase.JsonSerializerSettings);
				}
			}

			public List<RegisterType> Registers = null;

			[JsonProperty(PropertyName = "Max Servo Count")]
			public int MaxServoCount { get; set; } = 32;

			public double Period { get; set; } = 10.0;
		}

		static Settings FSettings = new Settings();

		public object Perform()
		{
			//Get ports
			var ports = PortPool.X.Ports;

			//Get database collection
			var collection = Database.Connection.X.GetCollection<Database.Registers>();

			//We get the register objects from the first servo we find in order to know addresses and sizes
			DynamixelSDKSharp.Servo firstServo = null;

			var report = new Dictionary<string, List<byte>>();

			Parallel.ForEach(ports, (portIterator) =>
			{
				var listOfServosToLog = new List<byte>();
				var port = portIterator.Value;

				// check which servos need to be logged for either high frequency or because they've never been logged
				foreach (var servoIterator in port.Servos)
				{
					var servoID = servoIterator.Key;
					var servo = servoIterator.Value;

					// store the first servo
					if (firstServo == null)
					{
						firstServo = servoIterator.Value;
					}
					else
					{
						//check all servos are the same model number
						if (servoIterator.Value.ProductSpecification.ModelNumber != firstServo.ProductSpecification.ModelNumber)
						{
							throw (new Exception("Mixed model numbers are not supported"));
						}
					}

					// get data log entries for this servo
					var documents = from document in collection.AsQueryable<Database.Registers>()
									where document.ServoID == servoID
									orderby document.TimeStamp descending
									select document;

					if (Database.HighFrequencyRequests.X.IsHighFrequencyNow(servoID))
					{
						// It's on high frequency list
						listOfServosToLog.Add(servoID);
					}
					else if (documents.Count() == 0)
					{
						// It has never been logged
						listOfServosToLog.Add(servoID);
					}
				}

				//check regular logging (choose oldest)
				{
					var documents = collection.AsQueryable()
									.OrderBy(row => row.TimeStamp)
									.GroupBy(group => new { group.ServoID })
									.Distinct()
									.Select(group => group.First().ServoID);

					//Ideally we don't want to call ToList first as this means we perform the following operation locally
					var servosWithOldData = documents.ToList().Distinct().Take(Logger.FSettings.MaxServoCount);

					foreach (var servoWithOldData in servosWithOldData)
					{
						listOfServosToLog.Add((byte)servoWithOldData);
					}
				}

				// accumulate data logs
				var recordedValues = new Dictionary<RegisterType, Dictionary<byte, int>>();

				if (listOfServosToLog.Count > 0)
				{
					foreach (var registerType in Logger.FSettings.Registers)
					{
						var registerInfo = firstServo.Registers[registerType];

						recordedValues.Add(registerType
							, portIterator.Value.GroupSyncRead(listOfServosToLog
								, registerInfo.Address
								, registerInfo.Size));
					}
				}

				//save data into database per servo
				foreach (var servoID in listOfServosToLog)
				{
					var valuesForServo = new Dictionary<string, int>();
					foreach (var registerValuesIterator in recordedValues)
					{
						valuesForServo.Add(registerValuesIterator.Key.ToString()
							, registerValuesIterator.Value[servoID]);
					}

					var row = new Database.Registers
					{
						ServoID = servoID,
						TimeStamp = DateTime.Now,
						RegisterValues = valuesForServo
					};

					collection.InsertOne(row);
				}
				
				//accumulate report log
				lock(report)
				{
					report.Add(port.Name, listOfServosToLog);
				}
			});

			return report;
		}
	}
}
