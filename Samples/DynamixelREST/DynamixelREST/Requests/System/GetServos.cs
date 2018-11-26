using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.System
{
	[Serializable]
	class GetServos : IRequest
	{
		public object Perform()
		{
			return PortPool.X.Servos;
		}
	}
}