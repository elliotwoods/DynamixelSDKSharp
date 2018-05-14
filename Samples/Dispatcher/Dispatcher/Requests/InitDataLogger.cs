using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[RequestHandler("/initDataLogger")]
	class InitDataLogger : IRequest
	{
		public object Perform()
		{
			DataLogger.Database.X.Connect();
			return new { };
		}
	}
}
