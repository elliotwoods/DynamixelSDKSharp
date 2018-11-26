using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelREST.Requests.Group
{
	[RequestHandler(Method = Method.POST | Method.GET)]
	class LogRegister : IRequest
	{
		public RegisterType register { get; set; }
		public List<int> servoIDs { get; set; }

		public object Perform()
		{
			//Get database collection
			var collection = Database.Connection.X.GetCollection<Database.Registers>();

			var servos = PortPool.X.Servos;

			if (this.servoIDs == null)
			{
				throw (new Exception("No list of servos provided. Please set servoIDs"));
			}

			//Check we have these servos
			foreach (var servoID in this.servoIDs)
			{
				if (!servos.ContainsKey(servoID))
				{
					throw (new Exception(String.Format("Cannot find servo {0}", servoID)));
				}
			}

			foreach (var servo in servos)
			{
				var servoID = servo.Key;

				var valuesForServo = new Dictionary<string, int>();
				valuesForServo.Add(register.ToString(), servo.Value.ReadValue(register));

				var row = new Database.Registers
				{
					ServoID = servoID,
					TimeStamp = DateTime.Now,
					RegisterValues = valuesForServo
				};

				collection.InsertOne(row);
			}

			return new { };
		}
	}
}
