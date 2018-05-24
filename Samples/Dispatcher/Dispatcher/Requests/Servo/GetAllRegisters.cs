using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("servo = {servo}")]
	class GetAllRegisters : IRequest
	{
		public int servo { get; set; }

		public object Perform()
		{
			var servo = PortPool.X.FindServo(this.servo);
			return servo.Registers;
		}
	}
}
