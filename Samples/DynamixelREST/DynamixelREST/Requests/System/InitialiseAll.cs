using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.System
{
	[Serializable]
	class InitialiseAll : IRequest
	{
		public object Perform()
		{
			PortPool.X.InitialiseAll();
			return new { };
		}
	}
}