using DynamixelSDKSharp;
using System;
using System.Diagnostics;
using System.Threading;

namespace Dispatcher.Requests.System
{
	[Serializable]
	class Reboot : IRequest
	{
		public object Perform()
		{
			var servos = PortPool.X.Servos;
			foreach(var iterator in servos)
			{
				var servo = iterator.Value;
				servo.Reboot();
			}
			Thread.Sleep(3000);
			PortPool.X.InitialiseAll();
			return new { };
		}
	}
}