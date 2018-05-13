using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
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