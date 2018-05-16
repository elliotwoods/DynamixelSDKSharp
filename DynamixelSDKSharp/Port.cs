using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelSDKSharp
{
	public enum ProtocolVersion
	{
		ProtocolVersion_1 = 1,
		ProtocolVersion_2 = 2
	}

	public enum BaudRate
	{
		BaudRate_9600 = 9600,
		BaudRate_57600 = 57600,
		BaudRate_115200 = 115200,
		BaudRate_1M = 1000000,
		BaudRate_2M = 2000000,
		BaudRate_3M = 3000000,
		BaudRate_4M = 4000000,
		BaudRate_4_5M = 4500000
	}

	public class Port : IDisposable
	{
		public bool IsOpen { get; private set; }
		public string Name { get; private set; }

		WorkerThread FWorkerThread;
		int FPortNumber;

		static bool FPacketHandlerInitialised = false;

		public ProtocolVersion ProtocolVersion = ProtocolVersion.ProtocolVersion_2;

		//when writing async, we want to overwrite any registers in the outbox as we go along
		private Dictionary<byte, Registers> Outbox = new Dictionary<byte, Registers>();

		private Dictionary<int, Servo> FServos = new Dictionary<int, Servo>();

		private void ThrowIfNotOpen()
		{
			if (!this.IsOpen)
			{
				throw (new Exception("Port is not open"));
			}
		}

		public Port(string portName, BaudRate baudRate = BaudRate.BaudRate_57600)
		{
			this.Name = portName;
			this.FWorkerThread = new WorkerThread("Port " + portName);
			this.FWorkerThread.DoSync(() => {
				try
				{
					// Open the port
					this.FPortNumber = NativeFunctions.portHandler(portName);
					if (!NativeFunctions.openPort(this.FPortNumber))
					{
						throw (new Exception("Failed to open port"));
					}

					// Initialise the packet handler
					if (!FPacketHandlerInitialised)
					{
						NativeFunctions.packetHandler();
						FPacketHandlerInitialised = true;
					}

					// Initialise the packet handler
					this.IsOpen = true;
					this.BaudRate = baudRate;
				}
				catch (Exception e)
				{
					NativeFunctions.clearPort(this.FPortNumber);
					this.IsOpen = false;
					throw (e);
				}
			});
		}

		public BaudRate BaudRate
		{
			get
			{
				this.ThrowIfNotOpen();
				int baudRateNumber = 0;
				this.FWorkerThread.DoSync(() => {
					baudRateNumber = NativeFunctions.getBaudRate(this.FPortNumber);
				});
				return (BaudRate)baudRateNumber;
			}

			set
			{
				this.ThrowIfNotOpen();
				this.FWorkerThread.DoSync(() => {
					if (!(NativeFunctions.setBaudRate(this.FPortNumber, (int)value)))
					{
						throw (new Exception("Failed to set baud rate"));
					}
				});
			}
		}

		public void Refresh()
		{
			List<byte> foundServoIDs = new List<byte>();

			this.FWorkerThread.DoSync(() =>
			{
				//send ping
				NativeFunctions.broadcastPing(this.FPortNumber, (int)this.ProtocolVersion);
				var result = NativeFunctions.getLastTxRxResult(this.FPortNumber, (int)this.ProtocolVersion);
				if (result != NativeFunctions.COMM_SUCCESS)
				{
					var exceptionString = Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult((int)this.ProtocolVersion, result));
					throw (new Exception("getTxRxResult : " + exceptionString));
				}

				//get ping results
				for (byte id = 0; id < NativeFunctions.MAX_ID; id++)
				{
					if (NativeFunctions.getBroadcastPingResult(this.FPortNumber, (int)this.ProtocolVersion, (int)id))
					{
						foundServoIDs.Add(id);
					}
				}
			});

			//add new found servos
			foreach(var servoID in foundServoIDs)
			{
				if(!this.FServos.ContainsKey(servoID))
				{
					var modelNumber = this.Read(servoID, 0, 2);
					var servo = new Servo(this, servoID, modelNumber);
					this.FServos.Add(servoID, servo);
				}
			}

			//remove any servos not seen any more
			{
				var toRemove = this.FServos.Where(pair => !foundServoIDs.Contains((byte)pair.Key))
								.Select(pair => pair.Key)
								.ToList();
				foreach(var key in toRemove)
				{
					this.FServos.Remove(key);
				}
			}
		}

		[JsonIgnore]
		public Dictionary<int, Servo> Servos
		{
			get
			{
				return this.FServos;
			}
		}

		public List<int> ServoIDs
		{
			get
			{
				return this.FServos.Keys.ToList();
			}
		}

		private void WriteOnThread(byte id, Register register)
		{
			switch (register.Size)
			{
				case 1:
					NativeFunctions.write1ByteTxRx(this.FPortNumber
						, (int)this.ProtocolVersion
						, id
						, register.Address
						, (byte)register.Value);
					break;
				case 2:
					NativeFunctions.write2ByteTxRx(this.FPortNumber
						, (int)this.ProtocolVersion
						, id
						, register.Address
						, (UInt16)register.Value);
					break;
				case 4:
					NativeFunctions.write4ByteTxRx(this.FPortNumber
						, (int)this.ProtocolVersion
						, id
						, register.Address
						, (UInt32)register.Value);
					break;
				default:
					throw (new Exception("Register size not supported"));
			}

			{
				var result = NativeFunctions.getLastTxRxResult(this.FPortNumber, (int)this.ProtocolVersion);
				if (result != (int)NativeFunctions.COMM_SUCCESS)
				{
					var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult((int)this.ProtocolVersion, result));
					throw (new Exception("getTxRxResult : " + errorMessage));
				}
			}

			{
				var error = NativeFunctions.getLastRxPacketError(this.FPortNumber, (int)this.ProtocolVersion);
				if (error != 0)
				{
					var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getRxPacketError((int)this.ProtocolVersion, error));
					throw (new Exception("getRxPacketError : " + errorMessage));
				}
			}
		}

		public void Write(byte id, Register register)
		{
			this.ThrowIfNotOpen();

			this.FWorkerThread.DoSync(() => {
				WriteOnThread(id, register);
			});
		}

		public void Write(byte id, Registers registers)
		{
			this.ThrowIfNotOpen();

			var registersCopy = registers.Clone() as Registers;

			this.FWorkerThread.DoSync(() => {
				foreach(var register in registersCopy.Values)
				{
					WriteOnThread(id, register);
				}
			});
		}

		private void WriteOutboxAsync()
		{
			this.FWorkerThread.Do(() => {
				Dictionary<byte, Registers> outboxCopy;
				lock (this.Outbox)
				{
					outboxCopy = this.Outbox;
					this.Outbox = new Dictionary<byte, Registers>();
				}

				foreach (var outboxIterator in outboxCopy)
				{
					var registers = outboxIterator.Value;
					foreach (var registerIterator in registers)
					{
						try
						{
							WriteOnThread(outboxIterator.Key, registerIterator.Value);
						}
						catch (Exception e)
						{
							Console.WriteLine("Exception writing {0}={1} on Servo #{2} : {3}"
								, registerIterator.Value.RegisterType.ToString()
								, registerIterator.Value.Value
								, outboxIterator.Key
								, e.Message);
						}
					}
				}
			});
		}

		public void WriteAsync(byte id, Register register)
		{
			this.ThrowIfNotOpen();

			//add the register to the outbox
			lock(this.Outbox)
			{
				//get the outbox registers set
				Registers registers = null;
				if(this.Outbox.ContainsKey(id))
				{
					registers = this.Outbox[id];
				}
				else
				{
					registers = new Registers();
					this.Outbox.Add(id, registers);
				}

				//add this register to outbox registers set
				if (registers.ContainsKey(register.RegisterType))
				{
					//Overwrite value if we already have it in the outbox
					registers[register.RegisterType] = register.Clone() as Register;
				}
				else
				{
					registers.Add(register.RegisterType, register.Clone() as Register);
				}
			}

			//write the outbox
			this.WriteOutboxAsync();
		}

		public void WriteAsync(byte id, Registers registers)
		{
			this.ThrowIfNotOpen();

			//add the register to the outbox
			lock (this.Outbox)
			{
				//get the outbox registers set
				Registers outboxRegisters = null;
				if (this.Outbox.ContainsKey(id))
				{
					outboxRegisters = this.Outbox[id];
				}
				else
				{
					outboxRegisters = new Registers();
					this.Outbox.Add(id, outboxRegisters);
				}

				//copy all the registers into the outbox registers set
				foreach(var registerIterator in registers)
				{
					if (outboxRegisters.ContainsKey(registerIterator.Key))
					{
						outboxRegisters[registerIterator.Key] = registerIterator.Value.Clone() as Register;
					}
					else
					{
						outboxRegisters.Add(registerIterator.Key, registerIterator.Value.Clone() as Register);
					}
				}
			}

			//write the outbox
			this.WriteOutboxAsync();
		}

		public void Read(byte id, Register register)
		{
			register.Value = this.Read(id, register.Address, register.Size);
		}

		public int Read(byte id, ushort address, int size)
		{
			this.ThrowIfNotOpen();
			int value = 0;
			this.FWorkerThread.DoSync(() =>
			{
				switch (size)
				{
					case 1:
						value = (int)NativeFunctions.read1ByteTxRx(this.FPortNumber
							, (int)this.ProtocolVersion
							, id
							, address);
						break;
					case 2:
						value = (int)NativeFunctions.read2ByteTxRx(this.FPortNumber
							, (int)this.ProtocolVersion
							, id
							, address);
						break;
					case 4:
						value = (int)NativeFunctions.read4ByteTxRx(this.FPortNumber
							, (int)this.ProtocolVersion
							, id
							, address);
						break;
					default:
						throw (new Exception(String.Format("Size {0} not supported for register size", size)));
				}

				{
					var result = NativeFunctions.getLastTxRxResult(this.FPortNumber, (int)this.ProtocolVersion);
					if (result != (int)NativeFunctions.COMM_SUCCESS)
					{
						var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult((int)this.ProtocolVersion, result));
						throw (new Exception("getTxRxResult : " + errorMessage));
					}
				}

				{
					var error = NativeFunctions.getLastRxPacketError(this.FPortNumber, (int)this.ProtocolVersion);
					if (error != 0)
					{
						var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getRxPacketError((int)this.ProtocolVersion, error));
						throw (new Exception("getRxPacketError : " + errorMessage));
					}
				}
			});

			return value;
		}

		public void Close()
		{
			this.ThrowIfNotOpen();
			this.FWorkerThread.DoSync(() =>
			{
				NativeFunctions.closePort(this.FPortNumber);
			});
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					this.FWorkerThread.Join();
				}

				this.FWorkerThread = null;
				if(this.IsOpen)
				{
					NativeFunctions.closePort(this.FPortNumber);
				}

				disposedValue = true;
			}
		}

		~Port() {
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}