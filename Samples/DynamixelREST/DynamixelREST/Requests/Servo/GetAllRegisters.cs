using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.Servo
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
			servo.ReadAll();
			return servo.Registers;
		}
	}
}
