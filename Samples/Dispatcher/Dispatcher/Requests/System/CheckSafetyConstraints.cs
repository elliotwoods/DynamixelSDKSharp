using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
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

			var exceptions = new Dictionary<byte, Exception>();

			foreach (var constraint in constraints)
			{
				foreach (var servo in servos)
				{
					if(Safety.ServoRestManager.X.IsResting(servo))
					{
						//ignore whilst resting
						continue;
					}

					try
					{
						//Check if constraint has been performed recently
						if (constraint.NeedsPerform(servo))
						{
							//If not, perform it now
							constraint.Perform(servo);
						}
					}
					catch(Exception e)
					{
						exceptions.Add(servo.ID, e);
					}
				}
			}

			return exceptions;
		}
	}
}