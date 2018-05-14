using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.DataLogger
{
	[DataRow("SystemLog")]
	class SystemLog
	{
		[BsonId]
		public ObjectId ID { get; set; }

		public string Module { get; set; }
		public Logger.Level LogLevel { get; set; }
		public string Message { get; set; }
		public object Exception { get; set; }
		public DateTime TimeStamp { get; set; } = DateTime.Now;

		public string AssemblyVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		public DateTime ProgramStart { get; set; } = Process.GetCurrentProcess().StartTime.ToUniversalTime();
	}
}
