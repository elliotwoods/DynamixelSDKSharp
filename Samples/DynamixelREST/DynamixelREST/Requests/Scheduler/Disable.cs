using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.Scheduler
{
	[Serializable]
	class Disable : IRequest
	{
		public object Perform()
		{
			DynamixelREST.Scheduler.X.Enabled = false;
			return new { };
		}
	}
}