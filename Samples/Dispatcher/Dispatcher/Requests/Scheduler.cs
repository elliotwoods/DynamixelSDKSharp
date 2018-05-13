using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[Serializable]
	class SchedulerEnable : IRequest
	{
		public object Perform()
		{
			Scheduler.X.Enabled = true;
			return Scheduler.X;
		}
	}

	[Serializable]
	class SchedulerDisable: IRequest
	{
		public object Perform()
		{
			Scheduler.X.Enabled = false;
			return Scheduler.X;
		}
	}
}