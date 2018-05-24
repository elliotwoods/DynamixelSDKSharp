using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.System
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