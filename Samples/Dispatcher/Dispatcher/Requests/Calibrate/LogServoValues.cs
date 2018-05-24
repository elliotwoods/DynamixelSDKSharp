using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Dispatcher.Requests.Calibrate
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	class LogServoValues : IRequest
	{
		public dynamic data { get; set; }
		public string filename { get; set; } = "calibrationLog.json";

		class LogEntry
		{
			public Dictionary<int, int> servoPositions { get; set; }
			public dynamic data { get; set; }
		}

		public object Perform()
		{
			var logEntry = new LogEntry();

			//get all servo positions
			logEntry.servoPositions = new Dictionary<int, int>();
			foreach(var servoIterator in PortPool.X.Servos)
			{
				logEntry.servoPositions.Add(servoIterator.Key, servoIterator.Value.ReadValue(RegisterType.PresentPosition));
			}

			//load existing log entries
			Dictionary<DateTime, LogEntry> logEntries = new Dictionary<DateTime, LogEntry>();
			try
			{
				using (StreamReader reader = new StreamReader(this.filename))
				{
					var jsonString = reader.ReadToEnd();
					JsonConvert.PopulateObject(jsonString, logEntries);
				}
			}
			catch(Exception e)
			{
				//do nothing
			}

			logEntry.data = this.data;

			//add this log entry
			logEntries.Add(DateTime.Now, logEntry);

			//save the log entries
			using (StreamWriter writer = new StreamWriter(filename))
			{
				var jsonString = JsonConvert.SerializeObject(logEntries, Formatting.Indented);
				writer.Write(jsonString);
			}
			
			return logEntries;
		}
	}
}