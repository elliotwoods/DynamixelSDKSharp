using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamixelSDKSharp;

namespace Dispatcher.Requests.Group
{
	[RequestHandler(Method = Method.POST)]
	class GotoByDatum : IRequest
	{
		public int PositionByDatum { get; set; } = 0;
		public int ServoIndexStart { get; set; } = 0;
		public int ServoIndexEnd { get; set; } = 0;

		public bool IgnoreMissingServos { get; set; } = true;
		public int Decimater { get; set; } = 1;
		public int DecimaterOffset { get; set; } = 0;

		public object Perform()
		{
			var servos = PortPool.X.Servos;

			//check if any servos are missing first
			if (!this.IgnoreMissingServos)
			{
				for (int index = this.ServoIndexStart; index <= ServoIndexEnd; index++)
				{
					if (!servos.ContainsKey(index))
					{
						throw (new Exception(String.Format("Missing Servo {0}", index)));
					}
				}
			}

			//get servo targets (sync)
			var targets = new Dictionary<int, int>();
			foreach (var servoIterator in servos)
			{
				if (this.Decimater > 1)
				{
					if (servoIterator.Key % this.Decimater != (DecimaterOffset % this.Decimater))
					{
						continue;
					}
				}

				if (servoIterator.Key >= this.ServoIndexStart && servoIterator.Key <= this.ServoIndexEnd)
				{
					var max = servoIterator.Value.ReadValue(RegisterType.MaxPositionLimit);
					var min = servoIterator.Value.ReadValue(RegisterType.MinPositionLimit);

					var position = (max + min) / 2 + this.PositionByDatum;
					targets.Add(servoIterator.Key, position);
				}
			}

			//move to targets (async)
			foreach (var target in targets)
			{
				var servo = servos[target.Key];
				servo.WriteValue(RegisterType.GoalPosition, target.Value);
			}

			return targets;
		}
	}
}