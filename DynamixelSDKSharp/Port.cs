using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

	[DebuggerDisplay("Name = {Name}, Address = {Address}, IsOpen = {IsOpen}")]
	public class Port : IDisposable
	{
		public bool IsOpen { get; private set; }
		public string Name { get; set; }
		public string Address { get; private set; }

		private int FPortNumber;

		private static bool FPacketHandlerInitialised = false;

		public ProtocolVersion ProtocolVersion = ProtocolVersion.ProtocolVersion_2;

		private Dictionary<byte, Servo> FServos = new Dictionary<byte, Servo>();

		static int MaximumGroupRequests = 8;

		private void ThrowIfNotOpen()
		{
			if (!this.IsOpen)
			{
				throw (new Exception("Port is not open"));
			}
		}

		public Port(string portAddress, BaudRate baudRate = BaudRate.BaudRate_57600)
		{
			this.Name = portAddress;
			this.Address = portAddress;
			this.BaudRate = baudRate;

			try
			{
				// Open the port
				this.FPortNumber = NativeFunctions.portHandler(portAddress);

				// Initialise the packet handler
				NativeFunctions.packetHandler();

				//Note that we don't call NativeFunctions.openPort() here because setting the baud rate achieves the same thing in the Dynamixel SDK.
				//NativeFunctions.setBaudRate(this.FPortNumber, (int)baudRate);

				this.IsOpen = true;
			}
			catch (Exception e)
			{
				NativeFunctions.clearPort(this.FPortNumber);
				this.IsOpen = false;
				throw (e);
			}
		}

		public BaudRate BaudRate { get; set; } = BaudRate.BaudRate_57600;

		public void Open()
		{
			// This causes an open
			if (!(NativeFunctions.setBaudRate(this.FPortNumber, (int)this.BaudRate)))
			{
				throw (new Exception("Failed to set baud rate"));
			}
		}

		public void Ping_Submit()
		{
			//send ping
			NativeFunctions.broadcastPing(this.FPortNumber, (int)this.ProtocolVersion);
			var result = NativeFunctions.getLastTxRxResult(this.FPortNumber, (int)this.ProtocolVersion);
			if (result != NativeFunctions.COMM_SUCCESS)
			{
				var exceptionString = Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult((int)this.ProtocolVersion, result));
				throw (new Exception("Ping_Submit : " + exceptionString));
			}
		}

		private void AddServo(byte servoID)
		{
			var modelNumber = this.Read(servoID, 0, 2);
			var servo = new Servo(this, servoID, modelNumber);
			this.FServos.Add(servoID, servo);
		}

		public void Ping_HandleResults()
		{
			List<byte> foundServoIDs = new List<byte>();

			//get ping results
			for (byte id = 0; id < NativeFunctions.MAX_ID; id++)
			{
				if (NativeFunctions.getBroadcastPingResult(this.FPortNumber, (int)this.ProtocolVersion, (int)id))
				{
					var modelNum = NativeFunctions.pingGetModelNum(this.FPortNumber, (int)this.ProtocolVersion, id);
					foundServoIDs.Add(id);
				}
			}

			//add new found servos
			foreach (var servoID in foundServoIDs)
			{
				if (!this.FServos.ContainsKey(servoID))
				{
					// Try reset if it's in hardware error state
					try
					{
						this.AddServo(servoID);
					}
					catch(Exception e)
					{
						Console.WriteLine(String.Format("Rebooting ({0}) on ({1})", servoID, this.Name));

						this.Reboot(servoID);
						Thread.Sleep(3000);

						this.AddServo(servoID);
					}
				}
			}

			//remove any servos not seen any more
			{
				var toRemove = this.FServos.Where(pair => !foundServoIDs.Contains((byte)pair.Key))
								.Select(pair => pair.Key)
								.ToList();
				foreach (var key in toRemove)
				{
					this.FServos.Remove(key);
				}
			}
		}

		public void Ping_ByModelNumber()
		{
			List<byte> foundServoIDs = new List<byte>();

			//get ping results
			for (byte servoID = 0; servoID < NativeFunctions.MAX_ID; servoID++)
			{
				try
				{
					var modelNumber = NativeFunctions.pingGetModelNum(this.FPortNumber, (int)this.ProtocolVersion, servoID);
					if (modelNumber != 0)
					{
						var servo = new Servo(this, servoID, modelNumber);
						this.FServos.Add(servoID, servo);
						foundServoIDs.Add(servoID);
					}
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
				}
				
			}

			// Reboot any that are in hardware error state
			{
				bool needsReboot = false;
				foreach(var iterator in this.Servos)
				{
					var servo = iterator.Value;
					if(servo.ReadValue(RegisterType.HardwareErrorStatus) != 0)
					{
						servo.Reboot();
						needsReboot = true;
					}
				}

				if(needsReboot)
				{
					Thread.Sleep(3000);
				}
			}

			//remove any servos not seen any more
			{
				var toRemove = this.FServos.Where(pair => !foundServoIDs.Contains((byte)pair.Key))
								.Select(pair => pair.Key)
								.ToList();
				foreach (var key in toRemove)
				{
					this.FServos.Remove(key);
				}
			}
		}

		public void Refresh()
		{
			this.Servos.Clear();

			//this.Ping_ByModelNumber();

			this.Ping_Submit();
			this.Ping_HandleResults();
		}

		[JsonIgnore]
		public Dictionary<byte, Servo> Servos
		{
			get
			{
				return this.FServos;
			}
		}

		public List<byte> ServoIDs
		{
			get
			{
				return this.FServos.Keys.ToList();
			}
		}

		private void CheckTxRxErrors()
		{
			{
				var result = NativeFunctions.getLastTxRxResult(this.FPortNumber, (int)this.ProtocolVersion);
				if (result != (int)NativeFunctions.COMM_SUCCESS)
				{
					var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult((int)this.ProtocolVersion, result));
					throw (new Exception("getLastTxRxResult : " + errorMessage));
				}
			}

			//{
			//	var error = NativeFunctions.getLastRxPacketError(this.FPortNumber, (int)this.ProtocolVersion);
			//	if (error != 0)
			//	{
			//		var errorMessage = Marshal.PtrToStringAnsi(NativeFunctions.getRxPacketError((int)this.ProtocolVersion, error));
			//		throw (new Exception("getLastRxPacketError : " + errorMessage));
			//	}
			//}
		}

		public void WriteSync(byte id, Register register)
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

			this.CheckTxRxErrors();
		}

		public void WriteSync(List<Servo> servos, RegisterType registerType)
		{
			if(servos.Count == 0)
			{
				return;
			}

			// We presume that all servos are of the same type
			var registerSize = (ushort) servos[0].Registers[registerType].Size;

			// Open the group write
			var groupNum = NativeFunctions.groupSyncWrite(this.FPortNumber
				, (int)this.ProtocolVersion
				, servos[0].Registers[registerType].Address
				, registerSize);

			// Fill the group write
			foreach(var servo in servos)
			{
				var result = NativeFunctions.groupSyncWriteAddParam(groupNum
					, servo.ID
					, (UInt32) servo.Registers[registerType].Value
					, registerSize);
				if(!result)
				{
					throw (new Exception("groupSyncWriteAddParam failed"));
				}
			}

			// Push the group write
			NativeFunctions.groupSyncWriteTxPacket(groupNum);

			// Check for errors
			this.CheckTxRxErrors();

			// Clear the parameter storage
			NativeFunctions.groupSyncWriteClearParam(groupNum);
		}

		class WriteAsyncBlock
		{
			public class Entry
			{
				public int Value;
				public ushort Size;
			}

			public byte servoID;
			public ushort StartAddress;
			public List<Entry> Entries = new List<Entry>();

			public ushort CurrentAddress
			{
				get
				{
					return (ushort)(this.StartAddress + this.DataLength);
				}
			}

			public ushort DataLength
			{
				get
				{
					ushort length = 0;
					foreach (var entry in this.Entries)
					{
						length += entry.Size;
					}
					return length;
				}
			}
		}

		void WriteAsync(IEnumerable<WriteAsyncBlock> writeAsyncBlocks)
		{
			// We can only handle a max count in one request, so split it if needed
			{
				var writeAsyncBlocksList = writeAsyncBlocks.ToList();
				if (writeAsyncBlocksList.Count > Port.MaximumGroupRequests)
				{
					var splitList = writeAsyncBlocksList
						.Select((x, i) => new { Index = i, Value = x })
						.GroupBy(x => x.Index / Port.MaximumGroupRequests)
						.Select(x => x.Select(v => v.Value).ToList())
						.ToList();

					foreach (var subList in splitList)
					{
						this.WriteAsync(subList);
					}

					return;
				}
			}

			this.ThrowIfNotOpen();

			// Initialise the group write
			var groupNum = NativeFunctions.groupBulkWrite(this.FPortNumber
				, (int)this.ProtocolVersion);


			foreach(var block in writeAsyncBlocks)
			{
				// Add the first entry in the block
				{
					var result = NativeFunctions.groupBulkWriteAddParam(groupNum
					, block.servoID
					, block.StartAddress
					, block.DataLength
					, (UInt32)block.Entries[0].Value
					, block.Entries[0].Size);
					if (!result)
					{
						throw (new Exception("Failed to write first block in groupBulkWriteAddParam"));
					}
				}

				var position = block.Entries[0].Size;

				// Add the remaining entries
				for(int i=1; i<block.Entries.Count; i++)
				{
					{
						var result = NativeFunctions.groupBulkWriteChangeParam(groupNum
						, block.servoID
						, block.StartAddress
						, block.DataLength
						, (UInt32)block.Entries[i].Value
						, block.Entries[i].Size
						, (ushort)position);

						if (!result)
						{
							throw (new Exception(String.Format("Failed to write ({0}) block in groupBulkWriteChangeParam", position)));
						}
					}
					position += block.Entries[i].Size;
				}
			}

			// Push the group write
			NativeFunctions.groupBulkWriteTxPacket(groupNum);

			// Check for errors
			this.CheckTxRxErrors();

			// Clear the parameter storage
			NativeFunctions.groupBulkWriteClearParam(groupNum);
		}

		public void WriteAsync(IEnumerable<WriteAsyncRequest> writeAsyncRequests)
		{
			var sortedRequests = writeAsyncRequests.ToList();
			sortedRequests.Sort((request1, request2) =>
				request1.Address.CompareTo(request2.Address));

			var blocksForAllServos = new Dictionary<byte, List<WriteAsyncBlock>>();

			// Gather request blocks for each servo
			foreach(var request in sortedRequests)
			{
				// Make list of blocks for this servo if none allocated
				if(!blocksForAllServos.ContainsKey(request.servo.ID))
				{
					blocksForAllServos.Add(request.servo.ID, new List<WriteAsyncBlock>());
				}

				var blocks = blocksForAllServos[request.servo.ID];

				// Check if we can add this address to any existing block
				bool foundBlock = false;
				foreach (var block in blocks)
				{
					if(block.CurrentAddress == request.Address)
					{
						block.Entries.Add(new WriteAsyncBlock.Entry
						{
							Value = request.Value
							, Size = request.Size
						});
						foundBlock = true;
						break;
					}
				}

				// Make a new block if one not already matching
				if(!foundBlock)
				{
					var block = new WriteAsyncBlock();
					block.servoID = request.servo.ID;
					block.StartAddress = request.Address;
					block.Entries.Add(new WriteAsyncBlock.Entry
					{
						Value = request.Value
						, Size = request.Size
					});
					blocks.Add(block);
				}
			}

			// Execute the request blocks
			// We must gather sets of blocks that contain each servo no more than once
			while(blocksForAllServos.Count != 0)
			{
				// Gather a set of blocks for different servos
				var blocks = new List<WriteAsyncBlock>();

				var blockSet = new List<WriteAsyncBlock>();

				// Take the last entry for each servo
				foreach(var iterator in blocksForAllServos)
				{
					blockSet.Add(iterator.Value[iterator.Value.Count - 1]);
				}

				// Execute the block set
				this.WriteAsync(blockSet);

				// Remove last entry from each blockSet
				foreach(var iterator in blocksForAllServos)
				{
					iterator.Value.RemoveAt(iterator.Value.Count - 1);
				}

				// Strip empty sets (rebuild dictionary)
				blocksForAllServos = blocksForAllServos.Where(pair => pair.Value.Count > 0)
								 .ToDictionary(pair => pair.Key,
											   pair => pair.Value);
			}
		}

		public void Read(byte id, Register register)
		{
			register.Value = this.Read(id, register.Address, register.Size);
		}

		public int Read(Servo servo, RegisterType registerType)
		{
			var register = servo.Registers[registerType];
			return this.Read(servo.ID, register.Address, register.Size);
		}

		public int Read(byte id, ushort address, int size)
		{
			this.ThrowIfNotOpen();
			int value = 0;
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

			this.CheckTxRxErrors();

			return value;
		}

		public List<int> ReadGroup(IEnumerable<Servo> servos, RegisterType registerType)
		{
			// We can only handle a max count in one request, so split it
			{
				var servosList = servos.ToList();
				if (servosList.Count > Port.MaximumGroupRequests)
				{
					var splitList = servos
						.Select((x, i) => new { Index = i, Value = x })
						.GroupBy(x => x.Index / Port.MaximumGroupRequests)
						.Select(x => x.Select(v => v.Value).ToList())
						.ToList();

					var resultsFromSplit = new List<int>();
					foreach(var subList in splitList)
					{
						var subResults = this.ReadGroup(subList, registerType);
						resultsFromSplit = resultsFromSplit.Concat(subResults).ToList();
					}
					return resultsFromSplit;
				}
			}

			this.ThrowIfNotOpen();

			// Initialise the group write
			var groupNum = NativeFunctions.groupBulkRead(this.FPortNumber
				, (int)this.ProtocolVersion);

			// Populate the request
			foreach (var servo in servos)
			{
				var register = servo.Registers[registerType];
				var result = NativeFunctions.groupBulkReadAddParam(groupNum
					, servo.ID
					, register.Address
					, (ushort) register.Size);
				if (!result)
				{
					throw (new Exception("groupBulkReadAddParam failed"));
				}
			}

			// Push the group read
			NativeFunctions.groupBulkReadTxRxPacket(groupNum);

			// Check for errors
			this.CheckTxRxErrors();

			// Get the data back out
			var results = new List<int>();
			foreach(var servo in servos)
			{
				var register = servo.Registers[registerType];
				if(!NativeFunctions.groupBulkReadIsAvailable(groupNum
					, servo.ID
					, register.Address
					, register.Size))
				{
					throw (new Exception("groupBulkReadIsAvailable returns false"));
				}

				var result = (int) NativeFunctions.groupBulkReadGetData(groupNum
					, servo.ID
					, register.Address
					, register.Size);

				results.Add(result);
			}

			// Clear the parameter storage
			NativeFunctions.groupBulkReadClearParam(groupNum);

			return results;
		}

		public void Reboot(byte servoID)
		{
			NativeFunctions.reboot(this.FPortNumber, (int) this.ProtocolVersion, servoID);
			this.CheckTxRxErrors();
		}

		public void Close()
		{
			this.ThrowIfNotOpen();
			NativeFunctions.closePort(this.FPortNumber);
			this.IsOpen = false;
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if(this.IsOpen)
				{
					this.Close();
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