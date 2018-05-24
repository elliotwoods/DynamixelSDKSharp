using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.System
{
	[RequestHandler(ThreadUsage = ThreadUsage.Exclusive)]
	[Serializable]
	class Refresh : IRequest
	{
		public object Perform()
		{
			PortPool.X.Refresh();
			return new {
				servoCount = PortPool.X.Servos.Count,
				portCount = PortPool.X.Ports.Count
			};
		}
	}
}