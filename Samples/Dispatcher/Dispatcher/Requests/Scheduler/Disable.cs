using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.Scheduler
{
	[Serializable]
	class Disable : IRequest
	{
		public object Perform()
		{
			Dispatcher.Scheduler.X.Enabled = false;
			return new { };
		}
	}
}