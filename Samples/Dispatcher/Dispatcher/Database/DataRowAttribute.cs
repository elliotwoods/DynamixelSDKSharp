using System;

namespace Dispatcher.Database
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
