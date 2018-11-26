using DynamixelSDKSharp;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelREST.Database.Models
{
	[DataRow("ManualCalibration")]
	class ManualCalibration : DataRow
	{
		public int HeliostatID { get; set; }

		public Dictionary<string, int> axis1ServoRegisters { get; set; } = new Dictionary<string, int>();
		public Dictionary<string, int> axis2ServoRegisters { get; set; } = new Dictionary<string, int>();

		public double InclinometerValue { get; set; }

		public static IEnumerable<ManualCalibration> GetLatestDocs()
		{
			var collection = Database.Connection.X.GetCollection<ManualCalibration>();
			var latestDocs = collection.AsQueryable()
				.OrderByDescending(doc => doc.TimeStamp)
				.GroupBy(doc => doc.HeliostatID)
				.Select(group => new ManualCalibration
				{
					HeliostatID = group.Key,
					axis1ServoRegisters = group.First().axis1ServoRegisters,
					axis2ServoRegisters = group.First().axis2ServoRegisters,
					InclinometerValue = group.First().InclinometerValue
				})
				.ToList();
			return latestDocs;
		}

		public static Dictionary<int, ManualCalibration> GetLatestPerHeliostat()
		{
			var latestDocs = ManualCalibration.GetLatestDocs();

			var latestPerHeliostat = new Dictionary<int, ManualCalibration>();
			foreach(var doc in latestDocs)
			{
				latestPerHeliostat.Add(doc.HeliostatID, doc);
			}

			return latestPerHeliostat;
		}
	}
}
