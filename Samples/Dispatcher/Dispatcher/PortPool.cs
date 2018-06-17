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
    }

	class PortPool
	{
		class ServoCache : Dictionary<string, List<byte>>
		{

		}

		//Singleton
		public static readonly PortPool X = new PortPool();

		//Lock - only repond function should handle this lock
		readonly public ReaderWriterLock Lock = new ReaderWriterLock();

		public Dictionary<string, Port> Ports { get; private set; } = new Dictionary<string, Port>();
		public SortedDictionary<int, Servo> Servos { get; private set; }  = new SortedDictionary<int, Servo>();

        private const string ConfigurationJsonFilename = "SerialPorts.json";
        private const string ServoCacheJsonFilename = "ServoCache.json";

		public PortPool()
		{
			// since we're static, we don't want to do anything which could cause an exception in initialisation
		}

		public void Refresh(bool useServoCache)
		{
			//HACK! override found ports and use a list from json
            using (StreamReader file = new StreamReader(ConfigurationJsonFilename))
            {
                var json = file.ReadToEnd();
				var portsConfig = JsonConvert.DeserializeObject<IEnumerable<PresetSerialPort>>(json);
				foreach (var portConfig in portsConfig) {
                    if (!this.Ports.ContainsKey(portConfig.PortAddress))
                    {
                        Logger.Log<PortPool>(Logger.Level.Trace, String.Format("Found port : {0} ({1})", portConfig.PortAddress, portConfig.PortName));

                        var port = new Port(portConfig.PortAddress, (BaudRate)portConfig.Baud);
						port.Name = portConfig.PortName;

                        Logger.Log<PortPool>(Logger.Level.Trace, String.Format("Connected to port : {0} (IsOpen = {1})", portConfig.PortAddress, port.IsOpen));

                        this.Ports.Add(portConfig.PortAddress, port);
                    } else
					{
						Logger.Log<PortPool>(Logger.Level.Trace, String.Format("Couldn't find port {0} ({1}) on system.", portConfig.PortAddress, portConfig.PortName));
					}
                }
            }

			//Use cached servo ID's if shift is pressed
			ServoCache servoCache = null;
			if(useServoCache)
			{
				using(StreamReader file = new StreamReader(ServoCacheJsonFilename))
				{
					var json = file.ReadToEnd();
					servoCache = JsonConvert.DeserializeObject<ServoCache>(json);
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
				Parallel.ForEach(this.Ports.Values, (port) =>
				{
					Logger.Log<PortPool>(Logger.Level.Trace, String.Format("Searching for servos on port {0}", port.Name));
					try
					{
						if(useServoCache && servoCache.ContainsKey(port.Address))
						{
							//use cache
							port.AddServos(servoCache[port.Address]);
						}
						else
						{
							//find servos
							port.Refresh();
						}

						var servos = port.Servos.Values;
						foreach(var servo in servos)
						{
							try
							{
								servo.ReadRegister(RegisterType.PresentPosition);
							}
							catch(Exception e)
							{
								servo.Reboot();
							}
						}
					}
					catch(Exception e)
					{
						Logger.Log(Logger.Level.Error
							, "Failed to find any servos"
							, String.Format("PortPool : {0} ({1})", port.Name, port.Address));
						Logger.Log(Logger.Level.Error
							, e
							, String.Format("PortPool : {0} ({1})", port.Name, port.Address));
					}
				});

				foreach (var port in this.Ports.Values)
				{
					var servosFound = new List<int>();

					foreach (var portServo in port.Servos)
					{
						if(this.Servos.ContainsKey(portServo.Key))
						{
							Logger.Log(Logger.Level.Warning
								, String.Format("2 servo have been found with the same ID ({0}) on ports {1} and {2}"
									, portServo.Key
									, portServo.Value.Port.Name
									, port.Name)
								, String.Format("PortPool : {0} ({1})", port.Name, port.Address));
						}
						else
						{
							this.Servos.Add(portServo.Key, portServo.Value);
							servosFound.Add(portServo.Key);
						}
					}

					var servosFoundStringList = servosFound.Select(x => x.ToString()).ToList();
					Logger.Log(Logger.Level.Trace
						, String.Format("Found servos: {0}", String.Join(", ", servosFoundStringList))
						, String.Format("PortPool : {0} ({1})", port.Name, port.Address));
				}

				Logger.Log<PortPool>(Logger.Level.Trace
						, String.Format("Total servos count : {0}", this.Servos.Count));
			}

			//save results to cache file
			{
				//rebuild the cache
				servoCache = new ServoCache();
				foreach(var port in this.Ports.Values)
				{
					servoCache.Add(port.Address, port.ServoIDs);
				}

				//save the cache
				using (var file = new StreamWriter(ServoCacheJsonFilename))
				{
					var json = JsonConvert.SerializeObject(servoCache);
					file.Write(json);
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
	}
}
