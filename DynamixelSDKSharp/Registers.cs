using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace DynamixelSDKSharp
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum RegisterType
	{
		NoValue,

		[EnumMember(Value = "Model Number")]
		ModelNumber,

		[EnumMember(Value = "Model Information")]
		ModelInformation,

		[EnumMember(Value = "Version of Firmware")]
		VersionOfFirmware,

		[EnumMember(Value = "ID")]
		ID,

		[EnumMember(Value = "Baud Rate")]
		BaudRate,

		[EnumMember(Value = "Return Delay Time")]
		ReturnDelayTime,

		[EnumMember(Value = "Drive Mode")]
		DriveMode,

		[EnumMember(Value = "Operating Mode")]
		OperatingMode,

		[EnumMember(Value = "Secondary(Shadow) ID")]
		SecondaryShadowID,

		[EnumMember(Value = "Protocol Version")]
		ProtocolVersion,

		[EnumMember(Value = "Homing Offset")]
		HomingOffset,

		[EnumMember(Value = "Moving Threshold")]
		MovingThreshold,

		[EnumMember(Value = "Temperature Limit")]
		TemperatureLimit,

		[EnumMember(Value = "Max Voltage Limit")]
		MaxVoltageLimit,

		[EnumMember(Value = "Min Voltage Limit")]
		MinVoltageLimit,

		[EnumMember(Value = "PWM Limit")]
		PWMLimit,

		[EnumMember(Value = "Current Limit")]
		CurrentLimit,

		[EnumMember(Value = "Acceleration Limit")]
		AccelerationLimit,

		[EnumMember(Value = "Velocity Limit")]
		VelocityLimit,

		[EnumMember(Value = "Max Position Limit")]
		MaxPositionLimit,

		[EnumMember(Value = "Min Position Limit")]
		MinPositionLimit,

		[EnumMember(Value = "Shutdown")]
		Shutdown,

		[EnumMember(Value = "Torque Enable")]
		TorqueEnable,

		[EnumMember(Value = "LED")]
		LED,

		[EnumMember(Value = "Status Return Level")]
		StatusReturnLevel,

		[EnumMember(Value = "Registered Instruction")]
		RegisteredInstruction,

		[EnumMember(Value = "Hardware Error Status")]
		HardwareErrorStatus,

		[EnumMember(Value = "Velocity I Gain")]
		VelocityIGain,

		[EnumMember(Value = "Velocity P Gain")]
		VelocityPGain,

		[EnumMember(Value = "Position D Gain")]
		PositionDGain,

		[EnumMember(Value = "Position I Gain")]
		PositionIGain,

		[EnumMember(Value = "Position P Gain")]
		PositionPGain,

		[EnumMember(Value = "Feedforward 2nd Gain")]
		Feedforward2ndGain,

		[EnumMember(Value = "Feedforward 1st Gain")]
		Feedforward1stGain,

		[EnumMember(Value = "Bus Watchdog")]
		BusWatchdog,

		[EnumMember(Value = "Goal PWM")]
		GoalPWM,

		[EnumMember(Value = "Goal Current")]
		GoalCurrent,

		[EnumMember(Value = "Goal Velocity")]
		GoalVelocity,

		[EnumMember(Value = "Profile Acceleration")]
		ProfileAcceleration,

		[EnumMember(Value = "Profile Velocity")]
		ProfileVelocity,

		[EnumMember(Value = "Goal Position")]
		GoalPosition,

		[EnumMember(Value = "Realtime Tick")]
		RealtimeTick,

		[EnumMember(Value = "Moving")]
		Moving,

		[EnumMember(Value = "Moving Status")]
		MovingStatus,

		[EnumMember(Value = "Present PWM")]
		PresentPWM,

		[EnumMember(Value = "Present Current")]
		PresentCurrent,

		[EnumMember(Value = "Present Velocity")]
		PresentVelocity,

		[EnumMember(Value = "Present Position")]
		PresentPosition,

		[EnumMember(Value = "Velocity Trajectory")]
		VelocityTrajectory,

		[EnumMember(Value = "Position Trajectory")]
		PositionTrajectory,

		[EnumMember(Value = "Present Input Voltage")]
		PresentInputVoltage,

		[EnumMember(Value = "Present Temperature")]
		PresentTemperature,

		[EnumMember(Value = "Indirect Address 1")] IndirectAddress1,
		[EnumMember(Value = "Indirect Address 2")] IndirectAddress2,
		[EnumMember(Value = "Indirect Address 3")] IndirectAddress3,
		[EnumMember(Value = "Indirect Address 4")] IndirectAddress4,
		[EnumMember(Value = "Indirect Address 5")] IndirectAddress5,
		[EnumMember(Value = "Indirect Address 6")] IndirectAddress6,
		[EnumMember(Value = "Indirect Address 7")] IndirectAddress7,
		[EnumMember(Value = "Indirect Address 8")] IndirectAddress8,
		[EnumMember(Value = "Indirect Address 9")] IndirectAddress9,
		[EnumMember(Value = "Indirect Address 10")] IndirectAddress10,
		[EnumMember(Value = "Indirect Address 11")] IndirectAddress11,
		[EnumMember(Value = "Indirect Address 12")] IndirectAddress12,
		[EnumMember(Value = "Indirect Address 13")] IndirectAddress13,
		[EnumMember(Value = "Indirect Address 14")] IndirectAddress14,
		[EnumMember(Value = "Indirect Address 15")] IndirectAddress15,
		[EnumMember(Value = "Indirect Address 16")] IndirectAddress16,
		[EnumMember(Value = "Indirect Address 17")] IndirectAddress17,
		[EnumMember(Value = "Indirect Address 18")] IndirectAddress18,
		[EnumMember(Value = "Indirect Address 19")] IndirectAddress19,
		[EnumMember(Value = "Indirect Address 20")] IndirectAddress20,
		[EnumMember(Value = "Indirect Address 21")] IndirectAddress21,
		[EnumMember(Value = "Indirect Address 22")] IndirectAddress22,
		[EnumMember(Value = "Indirect Address 23")] IndirectAddress23,
		[EnumMember(Value = "Indirect Address 24")] IndirectAddress24,
		[EnumMember(Value = "Indirect Address 25")] IndirectAddress25,
		[EnumMember(Value = "Indirect Address 26")] IndirectAddress26,
		[EnumMember(Value = "Indirect Address 27")] IndirectAddress27,
		[EnumMember(Value = "Indirect Address 28")] IndirectAddress28,
		[EnumMember(Value = "Indirect Address 29")] IndirectAddress29,
		[EnumMember(Value = "Indirect Address 30")] IndirectAddress30,
		[EnumMember(Value = "Indirect Address 31")] IndirectAddress31,
		[EnumMember(Value = "Indirect Address 32")] IndirectAddress32,
		[EnumMember(Value = "Indirect Address 33")] IndirectAddress33,
		[EnumMember(Value = "Indirect Address 34")] IndirectAddress34,
		[EnumMember(Value = "Indirect Address 35")] IndirectAddress35,
		[EnumMember(Value = "Indirect Address 36")] IndirectAddress36,
		[EnumMember(Value = "Indirect Address 37")] IndirectAddress37,
		[EnumMember(Value = "Indirect Address 38")] IndirectAddress38,
		[EnumMember(Value = "Indirect Address 39")] IndirectAddress39,
		[EnumMember(Value = "Indirect Address 40")] IndirectAddress40,
		[EnumMember(Value = "Indirect Address 41")] IndirectAddress41,
		[EnumMember(Value = "Indirect Address 42")] IndirectAddress42,
		[EnumMember(Value = "Indirect Address 43")] IndirectAddress43,
		[EnumMember(Value = "Indirect Address 44")] IndirectAddress44,
		[EnumMember(Value = "Indirect Address 45")] IndirectAddress45,
		[EnumMember(Value = "Indirect Address 46")] IndirectAddress46,
		[EnumMember(Value = "Indirect Address 47")] IndirectAddress47,
		[EnumMember(Value = "Indirect Address 48")] IndirectAddress48,
		[EnumMember(Value = "Indirect Address 49")] IndirectAddress49,
		[EnumMember(Value = "Indirect Address 50")] IndirectAddress50,
		[EnumMember(Value = "Indirect Address 51")] IndirectAddress51,
		[EnumMember(Value = "Indirect Address 52")] IndirectAddress52,
		[EnumMember(Value = "Indirect Address 53")] IndirectAddress53,
		[EnumMember(Value = "Indirect Address 54")] IndirectAddress54,
		[EnumMember(Value = "Indirect Address 55")] IndirectAddress55,
		[EnumMember(Value = "Indirect Address 56")] IndirectAddress56,

		[EnumMember(Value = "Indirect Data 1")] IndirectData1,
		[EnumMember(Value = "Indirect Data 2")] IndirectData2,
		[EnumMember(Value = "Indirect Data 3")] IndirectData3,
		[EnumMember(Value = "Indirect Data 4")] IndirectData4,
		[EnumMember(Value = "Indirect Data 5")] IndirectData5,
		[EnumMember(Value = "Indirect Data 6")] IndirectData6,
		[EnumMember(Value = "Indirect Data 7")] IndirectData7,
		[EnumMember(Value = "Indirect Data 8")] IndirectData8,
		[EnumMember(Value = "Indirect Data 9")] IndirectData9,
		[EnumMember(Value = "Indirect Data 10")] IndirectData10,
		[EnumMember(Value = "Indirect Data 11")] IndirectData11,
		[EnumMember(Value = "Indirect Data 12")] IndirectData12,
		[EnumMember(Value = "Indirect Data 13")] IndirectData13,
		[EnumMember(Value = "Indirect Data 14")] IndirectData14,
		[EnumMember(Value = "Indirect Data 15")] IndirectData15,
		[EnumMember(Value = "Indirect Data 16")] IndirectData16,
		[EnumMember(Value = "Indirect Data 17")] IndirectData17,
		[EnumMember(Value = "Indirect Data 18")] IndirectData18,
		[EnumMember(Value = "Indirect Data 19")] IndirectData19,
		[EnumMember(Value = "Indirect Data 20")] IndirectData20,
		[EnumMember(Value = "Indirect Data 21")] IndirectData21,
		[EnumMember(Value = "Indirect Data 22")] IndirectData22,
		[EnumMember(Value = "Indirect Data 23")] IndirectData23,
		[EnumMember(Value = "Indirect Data 24")] IndirectData24,
		[EnumMember(Value = "Indirect Data 25")] IndirectData25,
		[EnumMember(Value = "Indirect Data 26")] IndirectData26,
		[EnumMember(Value = "Indirect Data 27")] IndirectData27,
		[EnumMember(Value = "Indirect Data 28")] IndirectData28,
		[EnumMember(Value = "Indirect Data 29")] IndirectData29,
		[EnumMember(Value = "Indirect Data 30")] IndirectData30,
		[EnumMember(Value = "Indirect Data 31")] IndirectData31,
		[EnumMember(Value = "Indirect Data 32")] IndirectData32,
		[EnumMember(Value = "Indirect Data 33")] IndirectData33,
		[EnumMember(Value = "Indirect Data 34")] IndirectData34,
		[EnumMember(Value = "Indirect Data 35")] IndirectData35,
		[EnumMember(Value = "Indirect Data 36")] IndirectData36,
		[EnumMember(Value = "Indirect Data 37")] IndirectData37,
		[EnumMember(Value = "Indirect Data 38")] IndirectData38,
		[EnumMember(Value = "Indirect Data 39")] IndirectData39,
		[EnumMember(Value = "Indirect Data 40")] IndirectData40,
		[EnumMember(Value = "Indirect Data 41")] IndirectData41,
		[EnumMember(Value = "Indirect Data 42")] IndirectData42,
		[EnumMember(Value = "Indirect Data 43")] IndirectData43,
		[EnumMember(Value = "Indirect Data 44")] IndirectData44,
		[EnumMember(Value = "Indirect Data 45")] IndirectData45,
		[EnumMember(Value = "Indirect Data 46")] IndirectData46,
		[EnumMember(Value = "Indirect Data 47")] IndirectData47,
		[EnumMember(Value = "Indirect Data 48")] IndirectData48,
		[EnumMember(Value = "Indirect Data 49")] IndirectData49,
		[EnumMember(Value = "Indirect Data 50")] IndirectData50,
		[EnumMember(Value = "Indirect Data 51")] IndirectData51,
		[EnumMember(Value = "Indirect Data 52")] IndirectData52,
		[EnumMember(Value = "Indirect Data 53")] IndirectData53,
		[EnumMember(Value = "Indirect Data 54")] IndirectData54,
		[EnumMember(Value = "Indirect Data 55")] IndirectData55,
		[EnumMember(Value = "Indirect Data 56")] IndirectData56
	};

	public class Registers : Dictionary<RegisterType, Register>, ICloneable
	{
		public override string ToString()
		{
			string result = "";
			foreach (var register in this)
			{
				result = result + register.Value.ToString() + Environment.NewLine;
			}
			return result;
		}

		public object Clone()
		{
			var clone = new Registers();
			foreach (var pair in this)
			{
				clone.Add(pair.Key, (Register) pair.Value.Clone());
			}
			return clone;
		}
	}

	[DebuggerDisplay("Value = {Value}, RegisterType = {RegisterType}")]
	public class Register : ICloneable
	{
		public UInt16 Address { get; set; }
		public int Size { get; set; }
		[JsonProperty(PropertyName = "Data Name")]
		public string DataName { get; set; }
		public string Description { get; set; }
		public string Access { get; set; }
		public int Value { get; set; }

		[JsonProperty(PropertyName = "Register Type")]
		public RegisterType RegisterType { get; set; }

		public object Clone()
		{
			var clone = new Register
			{
				Address = this.Address,
				Size = this.Size,
				DataName = this.DataName,
				Description = this.Description,
				Access = this.Access,
				Value = this.Value,
				RegisterType = this.RegisterType
			};
			return clone;
		}

		public override string ToString()
		{
			return String.Format("{0} = {1} ({2} @{3} {4}b {5})", DataName, Value, Description, Address, Size, Access);
		}
	}
}