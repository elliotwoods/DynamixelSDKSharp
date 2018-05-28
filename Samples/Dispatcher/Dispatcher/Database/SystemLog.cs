namespace Dispatcher.Database
{
	[DataRow("SystemLog")]
	class SystemLog : DataRow
	{
		public string Module { get; set; }
		public Logger.Level LogLevel { get; set; }
		public string Message { get; set; }
		public object Exception { get; set; }
	}
}
