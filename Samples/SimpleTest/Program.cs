using System;
using System.Threading;
using DynamixelSDKSharp;

namespace SimpleTest
{
	class Program
	{

		static void Main(string[] args)
		{
			//change these settings
			var portName = "COM3";
			var baudRate = BaudRate.BaudRate_115200;

			//open the port
			Console.WriteLine("Opening port {0} using baud rate {1}", portName, baudRate.ToString());
			var port = new Port(portName, baudRate);

			//look for servos
			Console.WriteLine("Searching for servos");
			port.Refresh();

			//check if we found any servos
			if (port.Servos.Count == 0)
			{
				Console.WriteLine("No servos found. Please check the port settings, servo wiring and if your servo power is on.");
				return;
			}

			//list out servos we found
			foreach (var servoKeyValue in port.Servos)
			{
				var servoID = servoKeyValue.Key;
				var servo = servoKeyValue.Value;
				Console.WriteLine("Found servo ID = {0}, Model Name = {1}", servoID, servo.ProductSpecification.ModelName);
			}

			//check that ProductSpecification files loaded
			foreach (var servo in port.Servos.Values)
			{
				if(servo.Registers.Count == 0)
				{
					Console.WriteLine("Failed to load Registers for Model Number = {0}, Model Name = {1}, Config Filename = {2}. Please check file exists in right location."
						, servo.ProductSpecification.ModelNumber
						, servo.ProductSpecification.ModelName
						, servo.ProductSpecification.ConfigFilename);

				}
			}

			Console.WriteLine("Press any key to test moving servos. WARNING! Servos may move rapidly...");
			Console.ReadKey();

			//iterate through the servos
			foreach (var servoKeyValue in port.Servos)
			{
				var servoID = servoKeyValue.Key;
				var servo = servoKeyValue.Value;

				Console.WriteLine("Moving servo {0} to zero position", servoID);

				//set torque on
				servo.WriteValue(RegisterType.TorqueEnable, 1);

				//set velocity and acceleration
				servo.WriteValue(RegisterType.ProfileVelocity, 100);
				servo.WriteValue(RegisterType.ProfileAcceleration, 1000);

				//go to zero
				servo.WriteValue(RegisterType.GoalPosition, 0);
				while(servo.ReadValue(RegisterType.PresentPosition) > 10)
				{
					Thread.Sleep(1000);
				}

				Console.WriteLine("Moving forwards and backwards...");

				//move through all positions
				{
					//forwards
					for (int i = 0; i < 4096; i += 4)
					{
						servo.WriteValue(RegisterType.GoalPosition, i);
						Thread.Sleep(1);
						Console.Write(">");
					}

					//forwards
					for (int i = 4095; i > 0; i -= 4)
					{
						servo.WriteValue(RegisterType.GoalPosition, i);
						Thread.Sleep(1);
						Console.Write("<");
					}
				}
				Console.WriteLine();
			}

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();

			//close down cleanly (optional)
			port.Dispose();
		}
	}
}
