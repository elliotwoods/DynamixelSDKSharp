using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.System
{
	[Serializable]
	class ShutdownAll : IRequest
	{
		public object Perform()
		{
			PortPool.X.ShutdownAll();
			return new { };
		}
	}
}