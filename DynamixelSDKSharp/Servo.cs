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
		byte ID;
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

		public void Write(Register newValue)
		{
			var register = this.Registers[newValue.RegisterType];
			register.Value = newValue.Value;
			this.Port.WriteAsync(this.ID, register);
		}

		public void WriteSync(Register newValue)
		{
			var register = this.Registers[newValue.RegisterType];
			register.Value = newValue.Value;
			this.Port.Write(this.ID, register);
		}

		public Register Read(RegisterType registerType)
		{
			var register = this.Registers[registerType];
			this.Port.Read(this.ID, register);
			return register;
		}
	}
}
