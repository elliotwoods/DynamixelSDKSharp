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

		public object Perform()
		{
			var writeRequests = new List<WriteAsyncRequest>();

			foreach (var servoValue in this.servoValues)
			{
				var servo = PortPool.X.FindServo(servoValue.Key);
				servo.Registers[registerType].Value = servoValue.Value;

				writeRequests.Add(new WriteAsyncRequest
				{
					servo = servo
					, registerType = registerType
				});
			}

			PortPool.X.WriteAsync(writeRequests);

			return new { };
		}
	}
}