using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamixelSDKSharp;

namespace Dispatcher.Requests.Group
{
	[RequestHandler(Method = Method.POST | Method.GET)]
	class GetDeltas : IRequest
	{
		public int ServoIndexStart { get; set; } = 0;
		public int ServoIndexEnd { get; set; } = 0;
		public bool UseServoIndex { get; set; } = false;
		public bool TrimZeros { get; set; } = true;

		public object Perform()
		{
			var servos = PortPool.X.Servos;

			var deltas = new Dictionary<int, int>();

			foreach (var servoIterator in servos)
			{
				var servoIndex = servoIterator.Key;
				if(this.UseServoIndex)
				{
					if(servoIndex < ServoIndexStart || servoIndex > this.ServoIndexEnd)
					{
						continue;
					}
				}	

				var servo = servoIterator.Value;
				var presentPosition = servo.ReadValue(RegisterType.PresentPosition);
				var goalPosition = servo.ReadValue(RegisterType.GoalPosition);

				var delta = presentPosition - goalPosition;
				deltas.Add(servoIndex, delta);
			}

			if(this.TrimZeros)
			{
				deltas = deltas.Where(pair => pair.Value != 0)
					.ToDictionary(pair => pair.Key, pair => pair.Value);
			}

			return deltas;
		}
	}
}
