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
		public bool useMidvalue = true;

		protected override void Perform(DynamixelSDKSharp.Servo servo, Action<int> goToPosition, int startPosition)
		{

			if(useMidvalue)
			{
				//go to middle between limits 
				var min = servo.ReadValue(RegisterType.MinPositionLimit);
				var max = servo.ReadValue(RegisterType.MaxPositionLimit);

				goToPosition((max + min) / 2);
			}
			else
			{
				//go to middle of encoder values
				goToPosition(servo.ProductSpecification.EncoderResolution / 2);
			}
		}
	}
}