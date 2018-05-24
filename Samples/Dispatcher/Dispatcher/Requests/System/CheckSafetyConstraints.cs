using DynamixelSDKSharp;
using System;
using System.Diagnostics;

namespace Dispatcher.Requests.System
{
	[Serializable]
	class CheckSafetyConstraints : IRequest
	{
		public object Perform()
		{
			var constraints = Safety.Constraints.X;
			var servos = PortPool.X.Servos.Values;

			foreach (var constraint in constraints)
			{
				foreach (var servo in servos)
				{
					//Check if constraint has been performed recently
					if (constraint.NeedsPerform(servo))
					{
						//If not, perform it now
						constraint.Perform(servo);
					}
				}
			}

			return new { };
		}
	}
}