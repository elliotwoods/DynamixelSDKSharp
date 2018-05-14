using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[RequestHandler("/initialiseAll")]
	[Serializable]
	class InitialiseAll : IRequest
	{
		public object Perform()
		{
			PortPool.X.InitialiseAll();
			return new { };
		}
	}
}