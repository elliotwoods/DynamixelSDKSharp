using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Dispatcher.Database
{
	abstract class DataRow
	{
		[BsonId]
		public ObjectId ID { get; set; }

		public DateTime TimeStamp { get; set; } = DateTime.Now;
		public string AssemblyVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		public DateTime ProgramStart { get; set; } = Process.GetCurrentProcess().StartTime.ToUniversalTime();
	}
}
