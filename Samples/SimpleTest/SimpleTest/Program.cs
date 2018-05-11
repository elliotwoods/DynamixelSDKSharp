using System;
using System.Runtime.InteropServices;
using DynamixelSDKSharp;

namespace protocol_combined
{
	class ProtocolCombined
	{
		// Control table address for Dynamixel MX
		public const int ADDR_MX_TORQUE_ENABLE = 64;                  // Control table address is different in Dynamixel model
		public const int ADDR_MX_GOAL_POSITION = 116;
		public const int ADDR_MX_PRESENT_POSITION = 132;


		// Protocol version
		public const int PROTOCOL_VERSION1 = 2;                   // See which protocol version is used in the Dynamixel
		public const int PROTOCOL_VERSION2 = 2;

		// Default setting
		public const int DXL1_ID = 2;                   // Dynamixel ID: 1
		public const int BAUDRATE = 115200;
		public const string DEVICENAME = "COM6";              // Check which port is being used on your controller
															  // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

		public const int TORQUE_ENABLE = 1;                   // Value for enabling the torque
		public const int TORQUE_DISABLE = 0;                   // Value for disabling the torque
		public const int DXL1_MINIMUM_POSITION_VALUE = 100;                 // Dynamixel will rotate between this value
		public const int DXL1_MAXIMUM_POSITION_VALUE = 4000;                // and this value (note that the Dynamixel would not move when the position value is out of movable range. Check e-manual about the range of the Dynamixel you use.)
		public const int DXL1_MOVING_STATUS_THRESHOLD = 10;                  // Dynamixel MX moving status threshold

		public const byte ESC_ASCII_VALUE = 0x1b;

		public const int COMM_SUCCESS = 0;                                     // Communication Success result value
		public const int COMM_TX_FAIL = -1001;                                 // Communication Tx Failed

		static void Main(string[] args)
		{
			var port = new Port("COM6", BaudRate.BaudRate_115200);
			var servo = new Servo(port, 2, "../../../../../../AddressTables/MX-64_2.0.json");
			servo.ReadAll();

			//write out all addresses
			Console.Write(servo.Addresses.ToString());

			Console.WriteLine("Press any key to terminate...");
			Console.ReadKey();

			{
				var goalPosition = servo.Addresses.GoalPosition;
				for(int i=0; i<4096; i++)
				{
					goalPosition.Value = i;
					servo.Write(goalPosition);
				}
			}

			return;
			/*
			// Initialize PortHandler Structs
			// Set the port path
			// Get methods and members of PortHandlerLinux or PortHandlerWindows
			var port = NativeFunctions.portHandler(DEVICENAME);

			// Initialize PacketHandler Structs
			NativeFunctions.packetHandler();

			int index = 0;
			int dxl_comm_result = COMM_TX_FAIL;                                  // Communication result
			UInt32[] dxl1_goal_position = new UInt32[2] { DXL1_MINIMUM_POSITION_VALUE, DXL1_MAXIMUM_POSITION_VALUE };   // Goal position of Dynamixel MX-64 (2.0)

			byte dxl_error = 0;                                                  // Dynamixel error
			UInt32 dxl1_present_position = 0;                                    // Present position of Dynamixel MX

			// Open port
			if (NativeFunctions.openPort(port))
			{
				Console.WriteLine("Succeeded to open the port!");
			}
			else
			{
				Console.WriteLine("Failed to open the port!");
				Console.WriteLine("Press any key to terminate...");
				Console.ReadKey();
				return;
			}

			// Set port baudrate
			if (NativeFunctions.setBaudRate(port, BAUDRATE))
			{
				Console.WriteLine("Succeeded to change the baudrate!");
			}
			else
			{
				Console.WriteLine("Failed to change the baudrate!");
				Console.WriteLine("Press any key to terminate...");
				Console.ReadKey();
				return;
			}

			// Enable Dynamixel#1 torque
			NativeFunctions.write1ByteTxRx(port, PROTOCOL_VERSION1, DXL1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
			if ((dxl_comm_result = NativeFunctions.getLastTxRxResult(port, PROTOCOL_VERSION1)) != COMM_SUCCESS)
			{
				Console.WriteLine(Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult(PROTOCOL_VERSION1, dxl_comm_result)));
			}
			else if ((dxl_error = NativeFunctions.getLastRxPacketError(port, PROTOCOL_VERSION1)) != 0)
			{
				Console.WriteLine(Marshal.PtrToStringAnsi(NativeFunctions.getRxPacketError(PROTOCOL_VERSION1, dxl_error)));
			}
			else
			{
				Console.WriteLine("Dynamixel#{0} has been successfully connected ", DXL1_ID);
			}

			while (true)
			{
				Console.WriteLine("Press any key to continue! (or press ESC to quit!)");
				if (Console.ReadKey().KeyChar == ESC_ASCII_VALUE)
					break;

				// Write Dynamixel#1 goal position
				NativeFunctions.write4ByteTxRx(port, PROTOCOL_VERSION1, DXL1_ID, ADDR_MX_GOAL_POSITION, dxl1_goal_position[index]);
				if ((dxl_comm_result = NativeFunctions.getLastTxRxResult(port, PROTOCOL_VERSION1)) != COMM_SUCCESS)
				{
					Console.WriteLine(Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult(PROTOCOL_VERSION1, dxl_comm_result)));
				}
				else if ((dxl_error = NativeFunctions.getLastRxPacketError(port, PROTOCOL_VERSION1)) != 0)
				{
					Console.WriteLine(Marshal.PtrToStringAnsi(NativeFunctions.getRxPacketError(PROTOCOL_VERSION1, dxl_error)));
				}

				do
				{
					// Read Dynamixel#1 present position
					dxl1_present_position = NativeFunctions.read4ByteTxRx(port, PROTOCOL_VERSION1, DXL1_ID, ADDR_MX_PRESENT_POSITION);
					if ((dxl_comm_result = NativeFunctions.getLastTxRxResult(port, PROTOCOL_VERSION1)) != COMM_SUCCESS)
					{
						Console.WriteLine(Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult(PROTOCOL_VERSION1, dxl_comm_result)));
					}
					else if ((dxl_error = NativeFunctions.getLastRxPacketError(port, PROTOCOL_VERSION1)) != 0)
					{
						Console.WriteLine(Marshal.PtrToStringAnsi(NativeFunctions.getRxPacketError(PROTOCOL_VERSION1, dxl_error)));
					}

					Console.WriteLine("[ID: {0}] GoalPos: {1}  PresPos: {2}", DXL1_ID, dxl1_goal_position[index], dxl1_present_position);

				} while ((Math.Abs(dxl1_goal_position[index] - dxl1_present_position) > DXL1_MOVING_STATUS_THRESHOLD));

				// Change goal position
				if (index == 0)
				{
					index = 1;
				}
				else
				{
					index = 0;
				}
			}

			// Disable Dynamixel#1 Torque
			NativeFunctions.write1ByteTxRx(port, PROTOCOL_VERSION1, DXL1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
			if ((dxl_comm_result = NativeFunctions.getLastTxRxResult(port, PROTOCOL_VERSION1)) != COMM_SUCCESS)
			{
				Console.WriteLine(Marshal.PtrToStringAnsi(NativeFunctions.getTxRxResult(PROTOCOL_VERSION1, dxl_comm_result)));
			}
			else if ((dxl_error = NativeFunctions.getLastRxPacketError(port, PROTOCOL_VERSION1)) != 0)
			{
				Console.WriteLine(Marshal.PtrToStringAnsi(NativeFunctions.getRxPacketError(PROTOCOL_VERSION1, dxl_error)));
			}

			// Close port
			NativeFunctions.closePort(port);

			return;
			*/
		}
	}
}
