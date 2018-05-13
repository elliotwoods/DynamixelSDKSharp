using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
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