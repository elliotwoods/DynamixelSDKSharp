using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[RequestHandler("/getScheduler")]
	[Serializable]
	class GetScheduler : IRequest
	{
		public object Perform()
		{
			return Scheduler.X;
		}
	}

	[RequestHandler("/schedulerEnable")]
	[Serializable]
	class SchedulerEnable : IRequest
	{
		public object Perform()
		{
			Scheduler.X.Enabled = true;
			return new { };
		}
	}

	[RequestHandler("/schedulerDisable")]
	[Serializable]
	class SchedulerDisable: IRequest
	{
		public object Perform()
		{
			Scheduler.X.Enabled = false;
			return new { };
		}
	}
}