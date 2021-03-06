﻿using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.System
{
	[RequestHandler(ThreadUsage = ThreadUsage.Exclusive)]
	[Serializable]
	class Refresh : IRequest
	{
		public object Perform()
		{
			PortPool.X.Refresh(false);
			return new {
				servoCount = PortPool.X.Servos.Count,
				portCount = PortPool.X.Ports.Count
			};
		}
	}
}