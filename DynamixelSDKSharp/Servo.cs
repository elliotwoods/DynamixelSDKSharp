using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelSDKSharp
{
	public class Servo
	{
		Port Port;
		byte ID;

		public Servo(Port port, byte id, Registers registers)
		{
			this.Port = port;
			this.ID = id;
			this.Registers = registers;
			this.ReadAll();
		}

		public Servo(Port port, byte id, string addressTableFilename)
			: this(port, id, new Registers())
		{
			this.Registers.Load(addressTableFilename);
		}

		public Registers Registers;

		public void ReadAll()
		{
			foreach(var iterator in this.Registers)
			{
				this.Port.Read(this.ID, iterator.Value);
			}
		}

		public void WriteAll()
		{
			foreach(var iterator in this.Registers)
			{
				this.Port.Write(this.ID, iterator.Value);
			}
		}

		public void Write(Register newValue)
		{
			this.Port.WriteAsync(this.ID, newValue);
		}

		public void WriteSync(Register newValue)
		{
			this.Port.Write(this.ID, newValue);
		}
	}
}
