using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.DataLogger
{
	[AttributeUsage(AttributeTargets.Class)]
	class DataRowAttribute : Attribute
	{
		public string Collection { get; set; }

		public DataRowAttribute(string collection)
		{
			this.Collection = collection;
		}
	}
}
