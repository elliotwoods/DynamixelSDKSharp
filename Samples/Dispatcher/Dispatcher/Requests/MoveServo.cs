using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests
{
	[Serializable]
	[DebuggerDisplay("servo = {servo}, register = {register.RegisterType}, value = {register.Value}")]
	class MoveServo : IRequest
	{
		public int servo { get; set; }
		public int movement { get; set; }

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

			return new { };
		}
	}
}