using DynamixelSDKSharp;
using System;
using System.Diagnostics;
using System.Linq;

namespace Dispatcher.Requests.System
{
	[Serializable]
	class GetServoIDs : IRequest
	{
		public object Perform()
		{
			return PortPool.X.Servos.Keys.ToList();
		}
	}
}