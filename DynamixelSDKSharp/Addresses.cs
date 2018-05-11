using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelSDKSharp
{
	public class Addresses
	{
		#region Addresses
		[JsonProperty("Model Number")]
		public Channel ModelNumber { get; set; }

		[JsonProperty("Model Information")]
		public Channel ModelInformation { get; set; }

		[JsonProperty("Version of Firmware")]
		public Channel VersionofFirmware { get; set; }

		[JsonProperty("ID")]
		public Channel ID { get; set; }

		[JsonProperty("Baud Rate")]
		public Channel BaudRate { get; set; }

		[JsonProperty("Return Delay Time")]
		public Channel ReturnDelayTime { get; set; }

		[JsonProperty("Drive Mode")]
		public Channel DriveMode { get; set; }

		[JsonProperty("Operating Mode")]
		public Channel OperatingMode { get; set; }

		[JsonProperty("Secondary(Shadow) ID")]
		public Channel SecondaryShadowID { get; set; }

		[JsonProperty("Protocol Version")]
		public Channel ProtocolVersion { get; set; }

		[JsonProperty("Homing Offset")]
		public Channel HomingOffset { get; set; }

		[JsonProperty("Moving Threshold")]
		public Channel MovingThreshold { get; set; }

		[JsonProperty("Temperature Limit")]
		public Channel TemperatureLimit { get; set; }

		[JsonProperty("Max Voltage Limit")]
		public Channel MaxVoltageLimit { get; set; }

		[JsonProperty("Min Voltage Limit")]
		public Channel MinVoltageLimit { get; set; }

		[JsonProperty("PWM Limit")]
		public Channel PWMLimit { get; set; }

		[JsonProperty("Current Limit")]
		public Channel CurrentLimit { get; set; }

		[JsonProperty("Acceleration Limit")]
		public Channel AccelerationLimit { get; set; }

		[JsonProperty("Velocity Limit")]
		public Channel VelocityLimit { get; set; }

		[JsonProperty("Max Position Limit")]
		public Channel MaxPositionLimit { get; set; }

		[JsonProperty("Min Position Limit")]
		public Channel MinPositionLimit { get; set; }

		[JsonProperty("Shutdown")]
		public Channel Shutdown { get; set; }

		[JsonProperty("Torque Enable")]
		public Channel TorqueEnable { get; set; }

		[JsonProperty("LED")]
		public Channel LED { get; set; }

		[JsonProperty("Status Return Level")]
		public Channel StatusReturnLevel { get; set; }

		[JsonProperty("Registered Instruction")]
		public Channel RegisteredInstruction { get; set; }

		[JsonProperty("Hardware Error Status")]
		public Channel HardwareErrorStatus { get; set; }

		[JsonProperty("Velocity I Gain")]
		public Channel VelocityIGain { get; set; }

		[JsonProperty("Velocity P Gain")]
		public Channel VelocityPGain { get; set; }

		[JsonProperty("Position D Gain")]
		public Channel PositionDGain { get; set; }

		[JsonProperty("Position I Gain")]
		public Channel PositionIGain { get; set; }

		[JsonProperty("Position P Gain")]
		public Channel PositionPGain { get; set; }

		[JsonProperty("Feedforward 2nd Gain")]
		public Channel Feedforward2ndGain { get; set; }

		[JsonProperty("Feedforward 1st Gain")]
		public Channel Feedforward1stGain { get; set; }

		[JsonProperty("Bus Watchdog")]
		public Channel BusWatchdog { get; set; }

		[JsonProperty("Goal PWM")]
		public Channel GoalPWM { get; set; }

		[JsonProperty("Goal Current")]
		public Channel GoalCurrent { get; set; }

		[JsonProperty("Goal Velocity")]
		public Channel GoalVelocity { get; set; }

		[JsonProperty("Profile Acceleration")]
		public Channel ProfileAcceleration { get; set; }

		[JsonProperty("Profile Velocity")]
		public Channel ProfileVelocity { get; set; }

		[JsonProperty("Goal Position")]
		public Channel GoalPosition { get; set; }

		[JsonProperty("Realtime Tick")]
		public Channel RealtimeTick { get; set; }

		[JsonProperty("Moving")]
		public Channel Moving { get; set; }

		[JsonProperty("Moving Status")]
		public Channel MovingStatus { get; set; }

		[JsonProperty("Present PWM")]
		public Channel PresentPWM { get; set; }

		[JsonProperty("Present Current")]
		public Channel PresentCurrent { get; set; }

		[JsonProperty("Present Velocity")]
		public Channel PresentVelocity { get; set; }

		[JsonProperty("Present Position")]
		public Channel PresentPosition { get; set; }

		[JsonProperty("Velocity Trajectory")]
		public Channel VelocityTrajectory { get; set; }

		[JsonProperty("Position Trajectory")]
		public Channel PositionTrajectory { get; set; }

		[JsonProperty("Present Input Voltage")]
		public Channel PresentInputVoltage { get; set; }

		[JsonProperty("Present Temperature")]
		public Channel PresentTemperature { get; set; }
		#endregion Addresses

		public Dictionary<string, Channel> AsDictionary()
		{
			var dictionary = new Dictionary<string, Channel>();
			var type = typeof(Addresses);
			var properties = type.GetProperties();

			foreach (var property in properties)
			{
				var value = property.GetValue(this) as Channel;
				if (value != null)
				{
					dictionary.Add(value.DataName, value);
				}
			}

			return dictionary;
		}

		public Addresses()
		{
			
		}

		public override string ToString()
		{
			string result = "";
			var dictionary = this.AsDictionary();
			foreach(var address in dictionary)
			{
				result = result + address.Value.ToString() + Environment.NewLine;
			}
			return result;
		}

		public void Load(string filename)
		{
			using (StreamReader file = new StreamReader(filename))
			{
				var json = file.ReadToEnd();
				JsonConvert.PopulateObject(json, this);
			}
		}
	}

	[DebuggerDisplay("Value = {Value}, DataName = {DataName}")]
	public class Channel
	{
		public UInt16 Address { get; set; }
		public int Size { get; set; }

		[JsonProperty("Data Name")]
		public string DataName { get; set; }

		public string Description { get; set; }
		public string Access { get; set; }

		public UInt32 Value { get; set; }

		public override string ToString()
		{
			return String.Format("{0} = {1} ({2} @{3} {4}b {5})", DataName, Value, Description, Address, Size, Access);
		}
	}
}