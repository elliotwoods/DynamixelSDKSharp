using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Dispatcher.Requests.Calibrate
{

	[RequestHandler(Method = Method.POST)]
	[Serializable]
	class SetLimitSize : IRequest
	{
		public enum Selection
		{
			Odd
			, Even
			, All
		}

		public struct Report
		{
			public int priorMin;
			public int priorMax;
			public int priorRangeSize;
			public int newMin;
			public int newMax;
			public int newRangeSize;
		}

		public int rangeSize;
		public Selection selection;

		public object Perform()
		{
			var servos = PortPool.X.Servos;
			var reports = new Dictionary<int, Report>();

			foreach(var servo in servos)
			{
				switch(selection)
				{
					case Selection.Even:
						if(servo.Key % 2 == 1)
						{
							continue;
						}
						break;
					case Selection.Odd:
						if(servo.Key % 2 == 0)
						{
							continue;
						}
						break;
					case Selection.All:
					default:
						break;
				}

				var priorMin = servo.Value.ReadValue(RegisterType.MinPositionLimit);
				var priorMax = servo.Value.ReadValue(RegisterType.MaxPositionLimit);
				if(priorMin >= priorMax)
				{
					throw (new Exception(String.Format("Min {0} >= Max {1} for Servo #{2}", priorMin, priorMax, servo.Key)));
				}

				var middle = (priorMin + priorMax) / 2;
				var newMin = middle - rangeSize / 2;
				var newMax = newMin + rangeSize;

				if(newMin < 0)
				{
					newMin = 0;
				}
				if(newMax >= servo.Value.ProductSpecification.EncoderResolution)
				{
					newMax = servo.Value.ProductSpecification.EncoderResolution - 1;
				}

				servo.Value.WriteValue(RegisterType.TorqueEnable, 0);
				servo.Value.WriteValue(RegisterType.MinPositionLimit, newMin);
				servo.Value.WriteValue(RegisterType.MaxPositionLimit, newMax);

				var report = new Report();
				{
					report.priorMin = priorMin;
					report.priorMax = priorMax;
					report.priorRangeSize = priorMax - priorMin;
					report.newMin = newMin;
					report.newMax = newMax;
					report.newRangeSize = rangeSize;

					reports.Add(servo.Key, report);
				}
			}

			return reports;
		}
	}
}