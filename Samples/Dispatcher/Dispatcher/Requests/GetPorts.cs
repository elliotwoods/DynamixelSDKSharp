using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[RequestHandler("/getPorts")]
	[Serializable]
	class GetPorts : IRequest
	{
		public object Perform()
		{
			return PortPool.X.Ports;
		}
	}
}