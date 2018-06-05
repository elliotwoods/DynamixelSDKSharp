using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamixelSDKSharp;

namespace Dispatcher.Requests.Group
{
	[RequestHandler(Method = Method.POST)]
	class GotoByDatumMulti : IRequest
	{
		public List<int> PositionByDatum { get; set; } = new List<int>();
		public int ServoIndexStart { get; set; } = 0;
		public int ServoIndexEnd { get; set; } = 0;

		public bool IgnoreMissingServos { get; set; } = true;

		public object Perform()
		{
			var servos = PortPool.X.Servos;

			if(PositionByDatum.Count == 0)
			{
				throw (new Exception("This request requires a list of PositionByDatum values"));
			}

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
				if (servoIterator.Key >= this.ServoIndexStart && servoIterator.Key <= this.ServoIndexEnd)
				{
					var max = servoIterator.Value.ReadValue(RegisterType.MaxPositionLimit);
					var min = servoIterator.Value.ReadValue(RegisterType.MinPositionLimit);

					var positionByDatum = this.PositionByDatum[servoIterator.Key % this.PositionByDatum.Count];
					var absolutePosition = (max + min) / 2 + positionByDatum;
					targets.Add(servoIterator.Key, absolutePosition);
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