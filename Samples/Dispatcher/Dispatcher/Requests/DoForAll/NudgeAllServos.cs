using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading;

namespace Dispatcher.Requests.Test
{
	[RequestHandler(Method = Method.GET | Method.POST, ThreadUsage = ThreadUsage.Exclusive)]
	[Serializable]
	class NudgeAllServos : IDoForAllServos
	{
		public int amountDegrees { get; set; } = 45;

		protected override void Perform(DynamixelSDKSharp.Servo servo, Action<int> goToPosition, int startPosition)
		{
			//move by wave with amplitude 1/8 of a revolution (+/- 45 degrees)
			var movementAmount = (int)((float)servo.ProductSpecification.EncoderResolution / 8);

			//move +
			goToPosition(startPosition + (int)movementAmount);

			//move -
			goToPosition(startPosition - (int)movementAmount);

			//go back to start
			goToPosition(startPosition);
		}
	}
}