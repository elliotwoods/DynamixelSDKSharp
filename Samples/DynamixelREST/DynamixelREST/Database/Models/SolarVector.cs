using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelREST.Database.Models
{
	class SolarVector
	{
		[BsonId]
		public ObjectId ID { get; set; }

		public DateTime time { get; set; }
		public Vector3D vector {get; set;}
	}
}
