using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.Scheduler
{
	[Serializable]
	class Get : IRequest
	{
		public object Perform()
		{
			return Dispatcher.Scheduler.X;
		}
	}
}