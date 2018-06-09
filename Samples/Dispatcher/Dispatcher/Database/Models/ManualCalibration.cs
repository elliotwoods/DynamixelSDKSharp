using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Database.Models
{
	[DataRow("ManualCalibration")]
	class ManualCalibration : DataRow
	{
		public int HeliostatID { get; set; }

		public Dictionary<string, int> axis1ServoRegisters { get; set; } = new Dictionary<string, int>();
		public Dictionary<string, int> axis2ServoRegisters { get; set; } = new Dictionary<string, int>();

		public double InclinometerValue { get; set; }
	}
}
