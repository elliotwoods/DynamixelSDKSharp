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
	class GetRegisterValues : IRequest
	{
		public int servo { get; set; }
		public bool refresh { get; set; } = true;
		public List<RegisterType> registerTypes { get; set; }

		public object Perform()
		{
			var servo = PortPool.X.FindServo(this.servo);
			if (registerTypes == null)
			{
				throw (new Exception("No registerTypes selected"));
			}

			var registerValues = new Dictionary<RegisterType, int>();
			if (this.refresh)
			{
				foreach (var registerType in this.registerTypes)
				{
					registerValues.Add(registerType, servo.ReadValue(registerType));
				}
			}
			else
			{
				foreach (var registerType in this.registerTypes)
				{
					registerValues.Add(registerType, servo.Registers[registerType].Value);
				}
			}
			


			return registerValues;
		}
	}
}
