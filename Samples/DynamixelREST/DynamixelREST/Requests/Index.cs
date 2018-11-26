using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests
{
	[RequestHandler(CustomAddress = "/")]
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