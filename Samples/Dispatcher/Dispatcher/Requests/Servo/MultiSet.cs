using DynamixelSDKSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("servoIDs = {servoIDs}, register = {register}")]
	class MultiSet : IRequest
	{
		public Dictionary<int, int> servoValues;
		public RegisterType registerType;
		public bool synchronous = false;

		public object Perform()
		{
			foreach (var servoValue in this.servoValues)
			{
				var servo = PortPool.X.FindServo(servoValue.Key);
				servo.WriteValue(this.registerType
					, servoValue.Key
					, this.synchronous);
			}

			return new { };
		}
	}
}