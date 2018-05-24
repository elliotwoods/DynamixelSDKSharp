using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelSDKSharp
{
	public class Servo
	{
		public byte ID { get; private set; }
		public Port Port { get; private set; }
		public ProductSpecification ProductSpecification { get; private set; }

		public Servo(Port port, byte id, int modelNumber)
		{
			this.Port = port;
			this.ID = id;

			this.ProductSpecification = ProductDatabase.X.GetProductSpecification(modelNumber);

			this.Registers = (Registers)this.ProductSpecification.Registers.Clone();
			this.ReadAll();
		}
		
		[JsonIgnore]
		public Registers Registers;

		public void ReadAll()
		{
			foreach (var iterator in this.Registers)
			{
				this.Port.Read(this.ID, iterator.Value);
			}
		}

		public void WriteAll()
		{
			foreach (var iterator in this.Registers)
			{
				this.Port.Write(this.ID, iterator.Value);
			}
		}

		public void WriteValue(Register newValue, bool sync = false)
		{
			this.WriteValue(newValue.RegisterType, newValue.Value, sync);
		}

		public void WriteRegisters(Registers registers, bool sync = false)
		{
			var registersToWrite = new Registers();
			foreach(var register in registers)
			{
				//update our local register
				var ourRegister = this.Registers[register.Key];
				ourRegister.Value = register.Value.Value;

				//use our register for the write operations
				registersToWrite.Add(register.Key, ourRegister);
			}

			if (sync)
			{
				this.Port.Write(this.ID, registersToWrite);
			}
			else
			{
				this.Port.WriteAsync(this.ID, registersToWrite);
			}
		}

		public void WriteValue(RegisterType registerType, int value, bool sync = false)
		{
			var register = this.Registers[registerType];
			register.Value = value;
			if (sync)
			{
				this.Port.Write(this.ID, register);
			}
			else
			{
				this.Port.WriteAsync(this.ID, register);
			}
		}

		public Register ReadRegister(RegisterType registerType)
		{
			var register = this.Registers[registerType];
			this.Port.Read(this.ID, register);
			return register;
		}

		public int ReadValue(RegisterType registerType)
		{
			var register = this.ReadRegister(registerType);
			return register.Value;
		}
	}
}
