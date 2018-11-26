using System;
using DynamixelSDKSharp;
using Newtonsoft.Json;

namespace DynamixelREST.Models
{
	public class Heliostat
	{
		public int ID { get; set; }

		[JsonIgnore]
		public Servo axis1Servo { get; set; }

		[JsonIgnore]
		public Servo axis2Servo { get; set; }

		public int axis1ServoID { get; set; }

		public int axis2ServoID { get; set; }

		public HeliostatHAMParams hamParameters { get; set; }
	}
}
