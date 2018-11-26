using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.Scheduler
{
	[Serializable]
	class Get : IRequest
	{
		public object Perform()
		{
			return DynamixelREST.Scheduler.X;
		}
	}
}