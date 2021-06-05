using System;
using System.Collections.Generic;
using System.Linq;
using DynamixelSDKSharp;

namespace GroupReadWrite
{
	class Program
	{
		static void Main(string[] args)
		{
			//change these settings
			var portName = "COM15";
			var baudRate = BaudRate.BaudRate_115200;
			int iterations = 10;

			
			
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
			Console.WriteLine("Found {0} servos total", port.Servos.Count);
			foreach (var servoKeyValue in port.Servos)
			{
				var servoID = servoKeyValue.Key;
				var servo = servoKeyValue.Value;
				Console.WriteLine("Found servo ID = {0}, Model Name = {1}", servoID, servo.ProductSpecification.ModelName);
			}

			//check that ProductSpecification files loaded
			foreach (var servo in port.Servos.Values)
			{
				if (servo.Registers.Count == 0)
				{
					Console.WriteLine("Failed to load Registers for Model Number = {0}, Model Name = {1}, Config Filename = {2}. Please check file exists in right location."
						, servo.ProductSpecification.ModelNumber
						, servo.ProductSpecification.ModelName
						, servo.ProductSpecification.ConfigFilename);
				}
			}

			// Max a list of servos to get values for
			var servos = port.Servos.Values.ToList();


			// Read several times
			Console.WriteLine("Reading {0} times", iterations);
			for (int iteration = 0; iteration < iterations; iteration++)
			{
				// Group read temperature values
				{
					var temperatures = port.ReadGroup(servos, RegisterType.PresentPosition);
					for (int i = 0; i < temperatures.Count; i++)
					{
						Console.WriteLine("{0} : {1}", servos[i], temperatures[i]);
					}
				}

				Console.WriteLine("...");
			}

			// Write several times
			Console.WriteLine("Writing {0} times", iterations);
			{
				// Prepare the request (note that the value itself isn't part of the request, that must be set in the servo objecT)
				var torqueRequests = new List<WriteAsyncRequest>();
				foreach (var servo in servos)
				{
					torqueRequests.Add(new WriteAsyncRequest
					{
						servo = servo
						,
						registerType = RegisterType.TorqueEnable
					});
				}

				for (int iteration = 0; iteration < iterations; iteration++)
				{
					// Set torque off
					foreach (var servo in servos)
					{
						servo.Registers[RegisterType.TorqueEnable].Value = 0;
					}
					port.WriteAsync(torqueRequests);

					// Read back
					var torques = port.ReadGroup(servos, RegisterType.TorqueEnable);
					foreach (var torque in torques)
					{
						Console.Write(torque);
					}
					Console.WriteLine();

					// --

					// Set torque on
					foreach (var servo in servos)
					{
						servo.Registers[RegisterType.TorqueEnable].Value = 1;
					}
					port.WriteAsync(torqueRequests);

					// Read back
					torques = port.ReadGroup(servos, RegisterType.TorqueEnable);
					foreach (var torque in torques)
					{
						Console.Write(torque);
					}
					Console.WriteLine();
				}
			}
		}
	}
}
