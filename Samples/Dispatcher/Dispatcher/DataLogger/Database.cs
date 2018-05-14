using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.DataLogger
{
	class Database
	{
		readonly public static Database X = new Database();

		public bool Connected { get; private set; } = false;

		MongoClient FClient;
		IMongoDatabase FDatabase;

		private Database()
		{

		}

		public void Connect()
		{
			try
			{
				//Connect to the server
				this.FClient = new MongoClient();
				this.FDatabase = this.FClient.GetDatabase("Dispatcher");

				//Check if connected
				{
					this.Connected = (bool)this.FDatabase.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
				}

				//Serialize enums as strings
				{
					var pack = new ConventionPack
					{
						new EnumRepresentationConvention(BsonType.String)
					};
					ConventionRegistry.Register("EnumStringConvention", pack, t => true);
				}

				Logger.Log<Database>(Logger.Level.Trace, "--------------- SESION START : Database connected");
			}
			catch (Exception e)
			{
				Logger.Log<Database>(Logger.Level.Warning, "Failed to connect to MongoDB for DataLogging : " + e.Message);
				this.Connected = false;
				Logger.Log<Database>(Logger.Level.Trace, "Database not connected for logging. No MongoDB server found");
			}
		}

		public void Log<T>(T data)
		{
			if(this.Connected)
			{
				var dataRowAttribute = Attribute.GetCustomAttribute(typeof(T), typeof(DataRowAttribute)) as DataRowAttribute;
				var collection = this.FDatabase.GetCollection<T>(dataRowAttribute.Collection);

				collection.InsertOne(data);
			}
		}
	}
}
