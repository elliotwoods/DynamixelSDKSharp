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
	class GetFreshestValues : IRequest
	{
		public int servo { get; set; }

		public object Perform()
		{
			//Let's presume that data logging works properly here
			var collection = Database.Connection.X.GetCollection<Database.Registers>();

			var documents = from document in collection.AsQueryable()
							where document.ServoID == this.servo
							orderby document.TimeStamp descending
							select document;

			if (documents.Count() == 0)
			{
				throw (new Exception(String.Format("No data available for Servo #{0}", this.servo)));
			}
			else
			{
				return new
				{
					isHighFrequency = Database.HighFrequencyRequests.X.IsHighFrequencyNow(this.servo),
					document = documents.First()
				};
			}
		}
	}
}