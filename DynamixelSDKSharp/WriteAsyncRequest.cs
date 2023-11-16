using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelSDKSharp
{
	[DebuggerDisplay("Servo = {servo.ID}, RegisterType = {registerType}")]
	public struct WriteAsyncRequest
	{
		public Servo servo;
		public RegisterType registerType;
		public ushort Address
		{
			get
			{
				return this.servo.Registers[this.registerType].Address;
			}
		}

		public int Value
		{
			get
			{
				return this.servo.Registers[this.registerType].Value;
			}
		}

		public ushort Size
		{
			get
			{
				return (ushort) this.servo.Registers[this.registerType].Size;
			}
		}
	}
}
