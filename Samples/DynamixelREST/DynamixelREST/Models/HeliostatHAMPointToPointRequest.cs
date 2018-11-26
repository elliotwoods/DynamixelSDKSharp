using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace DynamixelREST.Models
{
	public sealed class HeliostatHamRequestCurrentPosition
	{
		//A1
		public int rotation { get; set; } = -1;

		//A2
		public int pitch { get; set; } = -1;
	}

	class HeliostatHAMPointToPointRequest
	{
		[JsonProperty(PropertyName = "params")]
		public Models.HeliostatHAMParams hamParameters { get; set; }

		public HeliostatHamRequestCurrentPosition currentServoSetting { get; set; } = new HeliostatHamRequestCurrentPosition();

		public Vector3 targetPoint { get; set; }

		public Vector3 source { get; set; }
	}
}
