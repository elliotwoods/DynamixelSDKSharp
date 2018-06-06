using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Models
{
	//This exists because System.Numeric's vector 3 has uppercase x, y, z components and the HAM needs them to be lower case and I can't be bothered to write custom JsonConverter.
	//TODO: Fix.
	public class Vector3
	{
		public float x { get; set; }
		public float y { get; set; }
		public float z { get; set; }
	}
}
