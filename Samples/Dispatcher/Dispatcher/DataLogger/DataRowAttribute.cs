using System;

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
