using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Dispatcher.Requests.Calibrate
{

	[RequestHandler(Method = Method.GET)]
	[Serializable]
	class GetLimits : IRequest
	{
		public struct Limit
		{
			public int min;
			public int max;
			public int range;
			public int mid;
		}


		public object Perform()
		{
			var servos = PortPool.X.Servos;
			var results = new Dictionary<int, Limit>();

			foreach (var iterator in servos) {
				var servo = iterator.Value;
				
				var min = servo.ReadValue(RegisterType.MinPositionLimit);
				var max = servo.ReadValue(RegisterType.MaxPositionLimit);
				var range = max - min;
				var mid = (min + max) / 2;

				var limit = new Limit
				{
					min = min
					, max = max
					, range = range
					, mid = mid
				};

				results.Add(iterator.Key, limit);
			}

			return results;
		}
	}
}