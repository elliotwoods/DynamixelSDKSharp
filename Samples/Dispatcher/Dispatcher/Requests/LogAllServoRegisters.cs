using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading;

namespace Dispatcher.Requests
{
	[RequestHandler("/logAllServoRegisters", Method = Method.GET)]
	[Serializable]
	class LogAllServoRegisters : IRequest
	{
		public object Perform()
		{
			var servos = PortPool.X.Servos;

			foreach (var servo in servos.Values)
			{
				var dataRow = new DataLogger.Registers();
				dataRow.servo = servo.ID;

				var registers = servo.Registers;
				foreach(var register in registers)
				{
					dataRow.registerValues.Add(register.Key.ToString(), register.Value.Value);
				}

				DataLogger.Database.X.Log(dataRow);
			}
			
			return new { };
		}
	}
}