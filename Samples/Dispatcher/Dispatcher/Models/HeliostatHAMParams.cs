using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Dispatcher.Models
{
	public class HAMAxisCalibration
	{
		public double m { get; set;} //gradient
		public double c { get; set;} 
		public double pitch { get; set; }
		public double rotation { get; set; }
	}

	public class HeliostatHAMParams
	{
		Vector3 pivotPoint { get; set; }
		HAMAxisCalibration pitchParameters { get; set; }
		HAMAxisCalibration rotationParameters { get; set; }
	}
}
