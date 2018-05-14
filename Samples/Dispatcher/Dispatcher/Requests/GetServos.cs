using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[RequestHandler("/getServos")]
	[Serializable]
	class GetServos : IRequest
	{
		public object Perform()
		{
			return PortPool.X.Servos;
		}
	}
}