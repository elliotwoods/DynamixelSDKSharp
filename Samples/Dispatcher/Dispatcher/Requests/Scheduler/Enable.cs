using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.Scheduler
{
	[Serializable]
	class Enable : IRequest
	{
		public object Perform()
		{
			Dispatcher.Scheduler.X.Enabled = true;
			return new { };
		}
	}
}