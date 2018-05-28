using DynamixelSDKSharp;
using System;
using System.Diagnostics;
using System.Threading;

namespace Dispatcher.Requests.Data
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("servo = {servo}, duration = {duration}")]
	class HighFrequencyRequest : IRequest
	{
		public int servo { get; set; }
		public double duration { get; set; } = 2;

		public object Perform()
		{
			Database.HighFrequencyRequests.X.AddRequest(this.servo, TimeSpan.FromSeconds(this.duration));
			return new { };
		}
	}
}