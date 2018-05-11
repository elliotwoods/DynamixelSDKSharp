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

		WorkerThread FWorkerThread;
		int FPortNumber;

		static bool FPacketHandlerInitialised = false;

		public ProtocolVersion ProtocolVersion = ProtocolVersion.ProtocolVersion_2;

		//when writing async, we want to overwrite any registers in the outbox as we go along
		private Dictionary<byte, Registers> Outbox = new Dictionary<byte, Registers>();

		private void ThrowIfNotOpen()
		{
			if (!this.IsOpen)
			{
				throw (new Exception("Port is not open"));
			}
		}

		public Port(string portName, BaudRate baudRate = BaudRate.BaudRate_57600)
		{
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
					throw (new Exception(""));
			}

			{
				var result = NativeFunctions.getLastTxRxResult(this.FPortNumber, (int)this.ProtocolVersion);
				if (result != (int)NativeFunctions.COMM_SUCCESS)
				{
					var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult((int)this.ProtocolVersion, result));
					throw (new Exception(errorMessage));
				}
			}

			{
				var error = NativeFunctions.getLastRxPacketError(this.FPortNumber, (int)this.ProtocolVersion);
				if (error != 0)
				{
					var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getRxPacketError((int)this.ProtocolVersion, error));
					throw (new Exception(errorMessage));
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
						WriteOnThread(outboxIterator.Key, registerIterator.Value);
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
					registers[register.RegisterType] = register;
				}
				else
				{
					registers.Add(register.RegisterType, register);
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
						outboxRegisters[registerIterator.Key] = registerIterator.Value;
					}
					else
					{
						outboxRegisters.Add(registerIterator.Key, registerIterator.Value);
					}
				}
			}

			//write the outbox
			this.WriteOutboxAsync();
		}

		public void Read(byte id, Register register)
		{
			this.ThrowIfNotOpen();
			this.FWorkerThread.DoSync(() =>
			{
				switch (register.Size)
				{
					case 1:
						register.Value = (int)NativeFunctions.read1ByteTxRx(this.FPortNumber
							, (int)this.ProtocolVersion
							, id
							, register.Address);
						break;
					case 2:
						register.Value = (int)NativeFunctions.read2ByteTxRx(this.FPortNumber
							, (int)this.ProtocolVersion
							, id
							, register.Address);
						break;
					case 4:
						register.Value = (int)NativeFunctions.read4ByteTxRx(this.FPortNumber
							, (int)this.ProtocolVersion
							, id
							, register.Address);
						break;
					default:
						throw (new Exception(""));
				}

				{
					var result = NativeFunctions.getLastTxRxResult(this.FPortNumber, (int)this.ProtocolVersion);
					if (result != (int)NativeFunctions.COMM_SUCCESS)
					{
						var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult((int)this.ProtocolVersion, result));
						throw (new Exception(errorMessage));
					}
				}

				{
					var error = NativeFunctions.getLastRxPacketError(this.FPortNumber, (int)this.ProtocolVersion);
					if (error != 0)
					{
						var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getRxPacketError((int)this.ProtocolVersion, error));
						throw (new Exception(errorMessage));
					}
				}
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