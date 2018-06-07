using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher
{
	class PivotsAndTargets
	{
		public class PivotAndTarget
		{
			public string ID { get; set; }
			public float PivotX { get; set; }
			public float PivotY { get; set; }
			public float PivotZ { get; set; }
			public float TargetX { get; set; }
			public float TargetY { get; set; }
			public int TargetZ { get; set; }
		}

		//Singleton
		public static readonly PivotsAndTargets X = new PivotsAndTargets();

		public const string Filename = "PivotsAndTargets.json";
		public List<PivotAndTarget> Heliostats = new List<PivotAndTarget>();

		private PivotsAndTargets()
		{
			this.Load(PivotsAndTargets.Filename);
		}

		void Load(string filename)
		{
			using (StreamReader file = new StreamReader(filename))
			{
				var json = file.ReadToEnd();
				JsonConvert.PopulateObject(json, this, ProductDatabase.JsonSerializerSettings);
			}
		}

		public PivotAndTarget GetForHeliostatByID(int id)
		{
			foreach(var heliostat in this.Heliostats)
			{
				int idThat;
				if(!int.TryParse(heliostat.ID, out idThat))
				{
					continue;
				}
				if(id == idThat)
				{
					return heliostat;
				}
			}

			throw (new Exception("Heliostat not found in PivotsAndTargets"));
		}
	}
}
