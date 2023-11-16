using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace DynamixelSDKSharp
{
	public class ProductSpecification
	{
		[JsonProperty(PropertyName = "Model Number")]
		public int ModelNumber { get; set; }

		[JsonProperty(PropertyName = "Model Name")]
		public string ModelName { get; set; }

		[JsonProperty(PropertyName = "Config Filename")]
		public string ConfigFilename { get; set; }

		[JsonProperty(PropertyName = "Encoder Resolution")]
		public int EncoderResolution { get; set; }

		[JsonIgnore]
		public Registers Registers { get; set; }
	}

	public class ProductDatabase
	{
		public static ProductDatabase X = new ProductDatabase();

		public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } },
			NullValueHandling = NullValueHandling.Ignore
		};

		public const string PathToProductData = "./ProductDatabase/";

		public List<ProductSpecification> ProductSpecifications { get; private set; } = new List<ProductSpecification>();

		ProductDatabase()
		{
			this.Load();
		}

		void Load()
		{
			//load list of models
			using (StreamReader file = new StreamReader(PathToProductData + "Products.json"))
			{
				var json = file.ReadToEnd();
				JsonConvert.PopulateObject(json, this, ProductDatabase.JsonSerializerSettings);
			}

			//load the addresses of each one
			foreach (var productSpecificaton in this.ProductSpecifications)
			{
				//load the config file for the servo
				using (StreamReader file = new StreamReader(PathToProductData + productSpecificaton.ConfigFilename))
				{
					var json = file.ReadToEnd();
					productSpecificaton.Registers = JsonConvert.DeserializeObject<Registers>(json, ProductDatabase.JsonSerializerSettings);
				}

				//set the RegisterType enum content
				foreach (var register in productSpecificaton.Registers)
				{
					register.Value.RegisterType = register.Key;
				}
			}
		}

		public ProductSpecification GetProductSpecification(int modelNumber)
		{
			foreach(var productSpecification in this.ProductSpecifications)
			{
				if(productSpecification.ModelNumber == modelNumber)
				{
					return productSpecification;
				}
			}
			throw (new Exception(String.Format("Model Number {0} not found", modelNumber)));
		}
	}
}