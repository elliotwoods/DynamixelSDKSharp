using DynamixelSDKSharp;
using System.Collections.Generic;

namespace Dispatcher.DataLogger
{
	[DataRow("Registers")]
	class Registers : DataRow
	{
		public int servo { get; set; }
		public Dictionary<string, int> registerValues { get; set; } = new Dictionary<string, int>();
	}
}
