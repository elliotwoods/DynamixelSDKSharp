using DynamixelSDKSharp;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace Dispatcher.Requests.Data
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("servo = {servo}")]
	class GetHistoricalValues : IRequest
	{
		public int servo { get; set; }
		public DateTime end { get; set; } = DateTime.Now;
		public DateTime start { get; set; } = DateTime.MinValue;

		public double duration
		{
			set
			{
				this.start = this.end - TimeSpan.FromSeconds(value);
			}
		}
		public double minimumPeriod = 0;


		public object Perform()
		{
			//Let's presume that data logging works properly here
			var collection = Database.Connection.X.GetCollection<Database.Registers>();

			var startUTC = this.start.ToUniversalTime();
			var endUTC = this.end.ToUniversalTime();

			var documents = from document in collection.AsQueryable()
							where document.ServoID == this.servo
							where document.TimeStamp <= endUTC
							where document.TimeStamp >= startUTC
							orderby document.TimeStamp ascending
							select document;

			if (documents.Count() == 0)
			{
				throw (new Exception(String.Format("No data available for Servo #{0}", this.servo)));
			}
			else
			{
				if (this.minimumPeriod == 0)
				{
					return new
					{
						isHighFrequency = Database.HighFrequencyRequests.X.IsHighFrequencyNow(this.servo),
						documents = documents.ToList()
					};
				} else {
					var trimmedList = documents.ToList();
					var lastRecordTime = DateTime.MinValue;
					var timeSpan = TimeSpan.FromSeconds(this.minimumPeriod);

					trimmedList.RemoveAll(document =>
					{
						var passes = document.TimeStamp - lastRecordTime >= timeSpan;
						if (passes)
						{
							lastRecordTime = document.TimeStamp;
						}
						return !passes;
					});

					return trimmedList;
				}
			}
		}
	}
}