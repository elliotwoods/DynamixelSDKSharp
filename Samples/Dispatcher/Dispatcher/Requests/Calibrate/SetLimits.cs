using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Dispatcher.Requests.Calibrate
{

	[RequestHandler(Method = Method.POST)]
	[Serializable]
	class SetLimits : IRequest
	{
		public struct Limit
		{
			public int min;
			public int max;
		}

		public Dictionary<int, Limit> limits = new Dictionary<int, Limit>();

		public object Perform()
		{
			var servos = PortPool.X.Servos;

			foreach(var limit in limits)
			{
				if(!servos.ContainsKey(limit.Key))
				{
					throw (new Exception(String.Format("Servo {0} not found", limit.Key)));
				}

				var servo = servos[limit.Key];
				
				servo.WriteValue(RegisterType.TorqueEnable, 0);
				servo.WriteValue(RegisterType.MinPositionLimit, limit.Value.min);
				servo.WriteValue(RegisterType.MaxPositionLimit, limit.Value.max);
			}

			return new { };
		}
	}
}