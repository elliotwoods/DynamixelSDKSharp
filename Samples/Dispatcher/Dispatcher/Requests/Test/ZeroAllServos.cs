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
	class ZeroAllServos : IDoForAllServos
	{
		protected override void Perform(DynamixelSDKSharp.Servo servo, Action<int> goToPosition, int startPosition)
		{
			//go to middle position
			goToPosition(servo.ProductSpecification.EncoderResolution / 2);
		}
	}
}