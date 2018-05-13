using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher
{
	public class Logger
	{
		public enum Level
		{
			Message,
			Warning,
			Error,
			FatalError
		};

		public static void Log(Level level, string message)
		{
			Console.WriteLine(level.ToString() + ": " + message);
		}

		public static void Log(Level level, Exception e)
		{
			Console.WriteLine(level.ToString() + ": " + e.Message);
		}
	}
}
