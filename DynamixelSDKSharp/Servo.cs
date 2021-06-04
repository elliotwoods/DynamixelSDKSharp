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
			this.WriteRegisters(this.Registers, false);
		}

		public void WriteValue(Register newValue)
		{
			this.WriteValue(newValue.RegisterType, newValue.Value);
		}

		public void WriteValue(RegisterType registerType, int value)
		{
			var register = this.Registers[registerType];
			register.Value = value;
			this.Port.WriteSync(this.ID, register);
		}

		public void WriteRegisters(Registers registers, bool sync = false)
		{
			if(sync)
			{
				foreach(var iterator in registers)
				{
					this.Port.WriteSync(this.ID, iterator.Value);
				}
			}
			else
			{
				var writeAsyncRequests = new List<WriteAsyncRequest>();
				foreach (var iterator in registers)
				{
					var writeAsyncRequest = new WriteAsyncRequest
					{
						servo = this,
						registerType = iterator.Key
					};
					writeAsyncRequests.Add(writeAsyncRequest);
				}
				this.Port.WriteAsync(writeAsyncRequests);
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
