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
	class GetRegister : IRequest
	{
		public int servo { get; set; }
		public bool refresh { get; set; } = true;
		public RegisterType registerType { get; set; }

		public object Perform()
		{
			var servo = PortPool.X.FindServo(this.servo);

			if (refresh)
			{
				//return a fresh value
				return servo.ReadValue(registerType);
			}
			else
			{
				//return a possibly cached value
				return servo.Registers[registerType].Value;
			}
		}
	}
}
