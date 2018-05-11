using System;
using System.Threading;
using DynamixelSDKSharp;

namespace SimpleTest
{
	class Program
	{ 

		static void Main(string[] args)
		{
			//open the port
			var port = new Port("COM6", BaudRate.BaudRate_115200);

			//setup a servo with an address table (ID = 2)
			var servo = new Servo(port, 2, "../../../../../AddressTables/MX-64_2.0.json");

			//write out data from all addresses
			{
				servo.ReadAll();
				Console.Write(servo.Registers.ToString());
			}

			//move through all positions
			{
				Console.WriteLine("Press any key to start moving servo...");
				Console.ReadKey();
				var goalPosition = servo.Registers[RegisterType.GoalPosition];
				for(int i=0; i<4096; i+= 5)
				{
					goalPosition.Value = i;
					servo.Write(goalPosition);
					Thread.Sleep(1);
				}
			}

			Console.WriteLine("Press any key to exit...");

			//close down cleanly (optional)
			port.Dispose();
		}
	}
}
