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

	public class Port
	{
		public bool IsOpen { get; private set; }
		int FPortNumber;

		static bool FPacketHandlerInitialised = false;

		public ProtocolVersion ProtocolVersion = ProtocolVersion.ProtocolVersion_2;

		private void ThrowIfNotOpen()
		{
			if (!this.IsOpen)
			{
				throw (new Exception("Port is not open"));
			}
		}

		public Port(string portName, BaudRate baudRate = BaudRate.BaudRate_57600)
		{
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
		}

		public BaudRate BaudRate
		{
			get
			{
				this.ThrowIfNotOpen();
				var baudRateNumber = NativeFunctions.getBaudRate(this.FPortNumber);
				return (BaudRate)baudRateNumber;
			}

			set
			{
				this.ThrowIfNotOpen();
				if (!(NativeFunctions.setBaudRate(this.FPortNumber, (int)value))) {
					throw (new Exception("Failed to set baud rate"));
				}
			}
		}

		public void Write(byte id, Channel channel)
		{
			this.ThrowIfNotOpen();

			switch (channel.Size)
			{
				case 1:
					NativeFunctions.write1ByteTxRx(this.FPortNumber
						, (int)this.ProtocolVersion
						, id
						, channel.Address
						, (byte)channel.Value);
					break;
				case 2:
					NativeFunctions.write2ByteTxRx(this.FPortNumber
						, (int)this.ProtocolVersion
						, id
						, channel.Address
						, (UInt16)channel.Value);
					break;
				case 4:
					NativeFunctions.write4ByteTxRx(this.FPortNumber
						, (int)this.ProtocolVersion
						, id
						, channel.Address
						, channel.Value);
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

		public void Read(byte id, Channel channel)
		{
			this.ThrowIfNotOpen();

			switch (channel.Size)
			{
				case 1:
					channel.Value = (UInt32)NativeFunctions.read1ByteTxRx(this.FPortNumber
						, (int)this.ProtocolVersion
						, id
						, channel.Address);
					break;
				case 2:
					channel.Value = (UInt32)NativeFunctions.read2ByteTxRx(this.FPortNumber
						, (int)this.ProtocolVersion
						, id
						, channel.Address);
					break;
				case 4:
					channel.Value = (UInt32)NativeFunctions.read4ByteTxRx(this.FPortNumber
						, (int)this.ProtocolVersion
						, id
						, channel.Address);
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
	}
}