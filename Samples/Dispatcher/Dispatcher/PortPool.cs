using Dispatcher.Requests;
using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher
{
	class PortPool
	{
		//Singleton
		public static PortPool X { get; private set; } = new PortPool();

		public Dictionary<string, Port> Ports { get; private set; } = new Dictionary<string, Port>();
		public Dictionary<int, Servo> Servos { get; private set; }  = new Dictionary<int, Servo>();

		public PortPool()
		{
			// since we're static, we don't want to do anything which could cause an exception in initialisation
		}

		public void Refresh()
		{
			//check if any new ports have been connected
			var currentSystemPorts = SerialPort.GetPortNames();
			{
				foreach (var portName in currentSystemPorts)
				{
					if (!this.Ports.ContainsKey(portName))
					{
						Logger.Log(Logger.Level.Message, String.Format("Found port : {0}", portName));

						var port = new Port(portName, BaudRate.BaudRate_115200);
						Logger.Log(Logger.Level.Message, String.Format("Connected to port : {0} (IsOpen = {1})", portName, port.IsOpen));

						this.Ports.Add(portName, port);
					}
				}
			}

			//check if any ports have become closed
			{
				//remove if Dynamixel SDK reports the port is closed, or the system doesn't report that the port exists any more
				var toRemove = this.Ports.Where(pair => !pair.Value.IsOpen || !currentSystemPorts.Contains(pair.Key))
					.Select(pair => pair.Key)
					.ToList();

				foreach (var key in toRemove)
				{
					var port = this.Ports[key];
					this.Ports.Remove(key);
					port.Close();
				}
			}

			//rebuild the list of servos
			{
				this.Servos.Clear();
				foreach (var port in this.Ports.Values)
				{
					foreach(var portServo in port.Servos)
					{
						if(this.Servos.ContainsKey(portServo.Key))
						{
							Logger.Log(Logger.Level.Warning
								, String.Format("2 servo have been found with the same ID ({0}) on ports {1} and {2}"
									, portServo.Key
									, portServo.Value.Port.Name
									, port.Name));
						}
						else
						{
							this.Servos.Add(portServo.Key, portServo.Value);
						}
					}
				}
			}
		}

		public int Count
		{
			get
			{
				return this.Ports.Count;
			}
		}

		public Servo FindServo(int servoID)
		{
			if (!this.Servos.ContainsKey(servoID))
			{
				//we didn't find our servo

				//search for servos
				this.Refresh();

				//try again
				if (!this.Servos.ContainsKey(servoID))
				{
					throw (new Exception(String.Format("Servo #{0} is not mapped to any serial port.", servoID)));
				}
			}

			//return if successful
			return this.Servos[servoID];
		}
	}
}
