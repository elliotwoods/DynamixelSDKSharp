using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[Serializable]
	[DebuggerDisplay("servo = {servo}, register = {register.RegisterType}, value = {register.Value}")]
	class SetRegister : IRequest
	{
		public int servo { get; set; }
		public Register register { get; set; }

		public object Perform()
		{
			//find the servo
			var servo = PortPool.X.FindServo(this.servo);

			//send the command (sync)
			servo.Write(this.register);

			return new { };
		}
	}
}
