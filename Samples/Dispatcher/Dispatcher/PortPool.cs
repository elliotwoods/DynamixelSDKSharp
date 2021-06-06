using Dispatcher.Requests;
using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dispatcher
{
    class PresetSerialPort
    {
        public string PortName { get; set; }
        public string PortAddress { get; set; }
        public int Baud { get; set; }

		public bool Enabled = true;
    }

	class PortPool
	{
		//Singleton
		public static readonly PortPool X = new PortPool();

		//Lock - only repond function should handle this lock
		readonly public ReaderWriterLock Lock = new ReaderWriterLock();

		public Dictionary<string, Port> Ports { get; private set; } = new Dictionary<string, Port>();
		public SortedDictionary<int, Servo> Servos { get; private set; }  = new SortedDictionary<int, Servo>();

        private const string ConfigurationJsonFilename = "SerialPorts.json";

		public PortPool()
		{
			// since we're static, we don't want to do anything which could cause an exception in initialisation
		}

		public void Refresh()
		{
			//HACK! override found ports and use a list from json
            using (StreamReader file = new StreamReader(ConfigurationJsonFilename))
            {
                var json = file.ReadToEnd();
                foreach (var p in JsonConvert.DeserializeObject<IEnumerable<PresetSerialPort>>(json)) {
					if(!p.Enabled)
					{
						// Ignore
						continue;
					}

                    if (!this.Ports.ContainsKey(p.PortAddress))
                    {
                        Logger.Log<PortPool>(Logger.Level.Trace, String.Format("Found port : {0} ({1})", p.PortAddress, p.PortName));

                        var port = new Port(p.PortAddress, (BaudRate)p.Baud);
						port.Name = p.PortName;


                        this.Ports.Add(p.PortAddress, port);
                    } else
					{
						Logger.Log<PortPool>(Logger.Level.Trace
							, String.Format("Couldn't open port {0} ({1}).", p.PortAddress, p.PortName));
					}
                }
            }

			//Open the ports
			foreach(var iterator in this.Ports)
			{
				try
				{
					iterator.Value.Open();
					Logger.Log<PortPool>(Logger.Level.Trace, String.Format("Connected to port : {0} (IsOpen = {1})", iterator.Key, iterator.Value.IsOpen));
				}
				catch (Exception e)
				{
					Logger.Log<PortPool>(Logger.Level.Trace
							, String.Format("Couldn't open port {0} ({1}).", iterator.Value.Name, iterator.Value.Address));
				}
			}

			//check if any ports have become closed
			{
				//remove if Dynamixel SDK reports the port is closed, or the system doesn't report that the port exists any more
				var toRemove = this.Ports.Where(pair => !pair.Value.IsOpen)
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
				foreach(var port in this.Ports.Values)
				{
					Logger.Log<PortPool>(Logger.Level.Trace, String.Format("Searching for servos on port {0}", port.Name));
					try
					{
						port.Refresh();
					}
					catch(Exception e)
					{
						Logger.Log<PortPool>(Logger.Level.Error
							, String.Format("Failed to refresh servos on ", port.Name));
						Logger.Log<PortPool>(Logger.Level.Error
							, e);
					}

					Logger.Log<PortPool>(Logger.Level.Trace
						, String.Format("Found {0} servos on port {1} : {2}", port.Servos.Count, port.Name, String.Join(", ", port.Servos.Keys.Select(x => x.ToString()))));
				}

				foreach (var port in this.Ports.Values)
				{
					var servosFound = new List<int>();

					foreach (var portServo in port.Servos)
					{
						if(this.Servos.ContainsKey(portServo.Key))
						{
							Logger.Log<PortPool>(Logger.Level.Warning
								, String.Format("2 servos have been found with the same ID ({0}) on ports {1} and {2}"
									, portServo.Key
									, portServo.Value.Port.Name
									, port.Name));
						}
						else
						{
							this.Servos.Add(portServo.Key, portServo.Value);
							servosFound.Add(portServo.Key);
						}
					}
				}
			}

			//initialise settings on servos
			this.InitialiseAll();
		}

		public void InitialiseAll()
		{
			//load InitialisationRegisters
			var initialiseRegisters = new
			{
				Registers = new List<Register>()
			};
			using (StreamReader file = new StreamReader("InitialiseRegisters.json"))
			{
				var json = file.ReadToEnd();
				JsonConvert.PopulateObject(json, initialiseRegisters, ProductDatabase.JsonSerializerSettings);
			}

			//set the initialisation register values on all Servos
			foreach (var servo in this.Servos.Values)
			{
				servo.WriteRegisters(new Registers(initialiseRegisters.Registers));
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
				throw (new Exception(String.Format("Servo #{0} is not mapped to any serial port.", servoID)));
			}

			//return if successful
			return this.Servos[servoID];
		}

		public void ShutdownAll()
		{
			foreach(var servo in this.Servos.Values)
			{
				servo.WriteValue(RegisterType.TorqueEnable, 0);
			}
		}

		public void WriteAsync(IEnumerable<WriteAsyncRequest> writeAsyncRequests)
		{
			var writeAsyncRequestsPerPorts = new Dictionary<string, List<WriteAsyncRequest>>();

			// Gather the requests by port
			foreach(var writeAsyncRequest in writeAsyncRequests)
			{
				// get the port
				var port = writeAsyncRequest.servo.Port;

				// make a list for this port if needed
				if(!writeAsyncRequestsPerPorts.ContainsKey(port.Address))
				{
					writeAsyncRequestsPerPorts.Add(port.Address, new List<WriteAsyncRequest>());
				}

				// get the list for this port
				var writeAsyncRequestsForThisPort = writeAsyncRequestsPerPorts[port.Address];
				writeAsyncRequestsForThisPort.Add(writeAsyncRequest);
			}

			// Push the requests
			foreach(var iterator in writeAsyncRequestsPerPorts)
			{
				this.Ports[iterator.Key].WriteAsync(iterator.Value);
			}
		}
	}
}
