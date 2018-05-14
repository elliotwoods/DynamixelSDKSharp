using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[RequestHandler("/getRegister", Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("servo = {servo}, register = {register.RegisterType}, value = {register.Value}")]
	class GetRegister : IRequest
	{
		public int servo { get; set; }
		public RegisterType registerType { get; set; }

		public object Perform()
		{
			var servo = PortPool.X.FindServo(this.servo);
			return servo.ReadRegister(this.registerType);
		}
	}
}