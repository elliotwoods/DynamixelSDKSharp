using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamixelSDKSharp;

namespace Dispatcher.Requests.Group
{
	[RequestHandler(Method = Method.POST)]
	class Goto : IRequest
	{
		public int Position { get; set; } = 2048;
		public int ServoIndexStart { get; set; } = 0;
		public int ServoIndexEnd { get; set; } = 0;
		public bool IgnoreMissingServos { get; set; } = true;
		public int Decimater { get; set; } = 1;

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

			//set position of servos
			foreach (var servoIterator in servos)
			{
				if (servoIterator.Key % this.Decimater != 0)
				{
					continue;
				}

				if (servoIterator.Key >= this.ServoIndexStart && servoIterator.Key <= this.ServoIndexEnd)
				{
					servoIterator.Value.WriteValue(RegisterType.GoalPosition, this.Position);
				}
			}

			return new { };
		}
	}
}