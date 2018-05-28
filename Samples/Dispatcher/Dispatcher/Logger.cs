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
			Trace,
			Warning,
			Error,
			FatalError
		};

		public static void Log<T>(Level level, string message)
		{
			Logger.Log(level, message, typeof(T));
		}

		public static void Log(Level level, string message, Type moduleType)
		{
			var moduleName = moduleType.Name;
			Logger.Log(level, message, moduleName);
		}

		public static void Log(Level level, string message, string moduleName)
		{
			Console.WriteLine(String.Format("{0} in [{1}] : {2}", level.ToString(), moduleName, message));

			Database.Connection.X.Log(new Database.SystemLog
			{
				Module = moduleName,
				Message = message
			});
		}

		public static void Log<T>(Level level, Exception e)
		{
			Logger.Log(level, e, typeof(T));
		}

		public static void Log(Level level, Exception e, Type moduleType)
		{
			var moduleName = moduleType.FullName;
			Logger.Log(level, e, moduleName);
		}

		public static void Log(Level level, Exception e, string moduleName)
		{
			Console.WriteLine(String.Format("{0} in [{1}] : {2}", level.ToString(), moduleName, e.Message));

			Database.Connection.X.Log(new Database.SystemLog
			{
				Module = moduleName,
				Message = e.Message,
				LogLevel = level,
				Exception = new
				{
					Traceback = e.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList(),
					Source = e.Source,
					TargetSite = e.TargetSite
				}
			});
		}
	}
}
