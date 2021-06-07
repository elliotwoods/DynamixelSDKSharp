using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("servo = {servo}")]
	class GetRegisterForAll : IRequest
	{
		public RegisterType register { get; set; }

		public object Perform()
		{
			var servos = PortPool.X.Servos;
			var results = new Dictionary<int, int>();

			foreach(var iterator in servos)
			{
				results.Add(iterator.Key, iterator.Value.ReadValue(this.register));
			}

			return results;
		}
	}
}
