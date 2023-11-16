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
	class SetRegisterForAll : IRequest
	{
		public RegisterType register { get; set; }

		public int value = -1;

		public object Perform()
		{
			if(value == -1)
			{
				throw (new Exception("Value is not set"));
			}

			var servos = PortPool.X.Servos;

			foreach (var iterator in servos)
			{
				iterator.Value.WriteValue(this.register, this.value);
			}

			return new { };
		}
	}
}
