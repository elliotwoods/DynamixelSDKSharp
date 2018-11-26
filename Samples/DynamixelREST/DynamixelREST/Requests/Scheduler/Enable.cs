using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.Scheduler
{
	[Serializable]
	class Enable : IRequest
	{
		public object Perform()
		{
			DynamixelREST.Scheduler.X.Enabled = true;
			return new { };
		}
	}
}