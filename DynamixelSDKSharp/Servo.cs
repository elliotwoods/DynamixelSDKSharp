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

		public Servo(Port port, byte id, Addresses addresses)
		{
			this.Port = port;
			this.ID = id;
			this.Addresses = addresses;
			this.ReadAll();
		}

		public Servo(Port port, byte id, string addressTableFilename)
			: this(port, id, new Addresses())
		{
			this.Addresses.Load(addressTableFilename);
		}

		public Addresses Addresses;

		public void ReadAll()
		{
			var addressDictionary = this.Addresses.AsDictionary();
			foreach(var iterator in addressDictionary)
			{
				this.Port.Read(this.ID, iterator.Value);
			}
		}

		public void WriteAll()
		{
			var addressDictionary = this.Addresses.AsDictionary();
			foreach (var iterator in addressDictionary)
			{
				this.Port.Write(this.ID, iterator.Value);
			}
		}

		public void Write(Channel newValue)
		{
			//check if we have this address on this servo
			var dictionary = Addresses.AsDictionary();
			if(dictionary.ContainsKey(newValue.DataName))
			{
				var ourValue = dictionary[newValue.DataName];
				if (ourValue != newValue)
				{
					//if this is not our instance, update it
					ourValue.Value = newValue.Value;
				}

				this.Port.Write(ourValue);
			}
			else
			{
				throw (new Exception("This servo does not have this Channel"));
			}
		}
	}
}
