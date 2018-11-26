using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.System
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