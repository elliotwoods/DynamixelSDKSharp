using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[RequestHandler("/refresh", ThreadUsage = ThreadUsage.Exclusive)]
	[Serializable]
	class Refresh : IRequest
	{
		public object Perform()
		{
			PortPool.X.Refresh();
			return new { };
		}
	}
}