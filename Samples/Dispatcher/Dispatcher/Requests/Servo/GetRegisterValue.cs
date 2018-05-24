using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("servo = {servo}, register = {register.RegisterType}, value = {register.Value}")]
	class GetRegisterValue : IRequest
	{
		public int servo { get; set; }
		public bool refresh { get; set; } = true;
		public RegisterType registerType { get; set; }

		public object Perform()
		{
			var servo = PortPool.X.FindServo(this.servo);
			if(refresh)
			{
				return servo.ReadValue(this.registerType);
			}
			else
			{
				return servo.Registers[this.registerType];
			}
		}
	}
}