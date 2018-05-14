using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[RequestHandler("/")]
	[Serializable]
	class Index : IRequest
	{
		public object Perform()
		{
			return new {
				ports = PortPool.X.Ports,
				servos = PortPool.X.Servos
			};
		}
	}
}