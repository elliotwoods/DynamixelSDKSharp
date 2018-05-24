using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method= Method.POST)]
	[Serializable]
	[DebuggerDisplay("servo = {servo}, register = {register.RegisterType}, value = {register.Value}")]
	class SetRegisterValue : IRequest
	{
		public int servo { get; set; }

		[JsonProperty(PropertyName = "Register Type")]
		public RegisterType registerType { get; set; }

		public int Value { get; set; }

		public object Perform()
		{
			var servo = PortPool.X.FindServo(this.servo);
			servo.WriteValue(this.registerType, this.Value);

			return new { };
		}
	}
}
