using DynamixelSDKSharp;
using System;
using System.Diagnostics;
using System.Threading;

namespace Dispatcher.Requests.Servo
{
	[RequestHandler(Method = Method.POST)]
	[Serializable]
	[DebuggerDisplay("servo = {servo}, register = {register.RegisterType}, value = {register.Value}")]
	class Move : IRequest
	{
		public int servo { get; set; }
		public int movement { get; set; }
		bool blockUntilComplete { get; set; } = true;
		int timeoutMS { get; set; } = 5000;
		int epsilon { get; set; } = 1;

		public object Perform()
		{
			var servo = PortPool.X.FindServo(this.servo);
			var presentPosition = servo.ReadValue(RegisterType.PresentPosition);
			var newPosition = presentPosition + this.movement;

			//clamp the value to valid range
			{
				if(newPosition < 0)
				{
					newPosition = 0;
				}
				else if(newPosition >= servo.ProductSpecification.EncoderResolution)
				{
					newPosition = servo.ProductSpecification.EncoderResolution - 1;
				}
			}

			//update goal position
			servo.WriteValue(RegisterType.GoalPosition, newPosition);

			//block if we want to until move completed
			if (this.blockUntilComplete)
			{
				var timeStart = DateTime.Now;
				while (Math.Abs(servo.ReadValue(RegisterType.PresentPosition) - newPosition) > this.epsilon)
				{
					//check if we've timed out trying to get there
					if(DateTime.Now - timeStart > TimeSpan.FromMilliseconds(this.timeoutMS))
					{
						throw (new Exception("Timeout moving to goal position"));
					}

					Thread.Sleep(10);
				}
			}

			return new { };
		}
	}
}