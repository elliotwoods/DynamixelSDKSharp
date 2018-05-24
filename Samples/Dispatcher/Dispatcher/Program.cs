using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Nancy.Hosting.Self;

namespace Dispatcher
{
	class Program
	{
		public const int Port = 8000;
		static void Main()
		{
			HostConfiguration hostConfiguration = new HostConfiguration();
			{
				hostConfiguration.UrlReservations.CreateAutomatically = true;
				hostConfiguration.RewriteLocalhost = true;
			}

			var uriStrings = new List<string>
			{
				String.Format("http://localhost:{0}", Program.Port)
			};

			var uris = uriStrings
				.Select(uriString => new Uri(uriString))
				.ToArray();

			//start host
			using (var host = new NancyHost(hostConfiguration, uris))
			{
				host.Start();

				Console.WriteLine("Nancy now listening on:");
				foreach (var uriString in uriStrings)
				{
					Console.WriteLine(uriString);
				}

				//start scheduler (it will start with making requests to the host, so must be after host starts)
				Scheduler.X.Start();

				while(true)
				{
					Thread.Sleep(10000);
				}
			}
		}
	}
}
