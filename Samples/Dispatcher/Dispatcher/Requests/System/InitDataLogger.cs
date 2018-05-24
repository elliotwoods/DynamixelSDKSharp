using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.System
{
	class InitDataLogger : IRequest
	{
		public object Perform()
		{
			DataLogger.Database.X.Connect();
			return new { };
		}
	}
}
