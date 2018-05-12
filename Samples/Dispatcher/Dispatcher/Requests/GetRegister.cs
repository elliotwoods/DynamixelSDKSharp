using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[Serializable]
	[DebuggerDisplay("servo = {servo}, register = {register.RegisterType}, value = {register.Value}")]
	class GetRegister : IRequest
	{
		public int servo { get; set; }
		public RegisterType register { get; set; }
		public bool valueOnly { get; set; } = false;

		public object Perform()
		{
			//find the servo
			var servo = PortPool.X.FindServo(this.servo);

			//send the command (sync)
			var register = servo.Read(this.register);

			if (valueOnly)
			{
				return register.Value;
			}
			else
			{
				return register;
			}
		}
	}
}