using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[Serializable]
	[DebuggerDisplay("servo = {servo}, register = {register.RegisterType}, value = {register.Value}")]
	class SetRegisterValue : IRequest
	{
		public int servo { get; set; }
		public RegisterType registerType { get; set; }
		public int value { get; set; }

		public object Perform()
		{
			var servo = PortPool.X.FindServo(this.servo);
			servo.WriteValue(this.registerType, this.value);

			return new { };
		}
	}
}
