using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
		
		public Vector3 pivotPoint { get; set; }
		public HAMAxisCalibration pitchParameters { get; set; }
		public HAMAxisCalibration rotationParameters { get; set; }
	}
}
