using DynamixelSDKSharp;
using System.Collections.Generic;

namespace Dispatcher.Database
{
	[DataRow("Registers")]
	class Registers : DataRow
	{
		public int ServoID { get; set; }
		public Dictionary<string, int> RegisterValues { get; set; } = new Dictionary<string, int>();
	}
}
