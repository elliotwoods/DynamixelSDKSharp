using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[Serializable]
	class GetPorts : IRequest
	{
		public object Perform()
		{
			return PortPool.X.Ports;
		}
	}
}