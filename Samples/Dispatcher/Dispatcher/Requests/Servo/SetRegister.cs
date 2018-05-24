using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("servo = {servo}, register = {register.RegisterType}, value = {register.Value}")]
	class SetRegister : IRequest
	{
		public int servo { get; set; }
		public Register register { get; set; }

		public object Perform()
		{
			var servo = PortPool.X.FindServo(this.servo);
			servo.WriteValue(this.register);

			return new { };
		}
	}
}
