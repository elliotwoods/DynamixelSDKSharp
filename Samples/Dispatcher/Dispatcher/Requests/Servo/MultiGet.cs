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
	class MultiGet : IRequest
	{
		public List<int> servoIDs;
		public RegisterType registerType;

		public object Perform()
		{
			var result = new List<int>();

			foreach(var servoID in this.servoIDs) {
				var servo = PortPool.X.FindServo(servoID);
				result.Add(servo.ReadValue(this.registerType));
			}

			return result;
		}
	}
}