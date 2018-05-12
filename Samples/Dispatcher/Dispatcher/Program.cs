namespace Nancy.Demo.Hosting.Self
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Nancy.Hosting.Self;

	class Program
	{
		static void Main()
		{
			HostConfiguration hostConfiguration = new HostConfiguration();
			hostConfiguration.UrlReservations.CreateAutomatically = true;
			hostConfiguration.RewriteLocalhost = true;

			var uriStrings = new List<string>
			{
				"http://localhost:8000",
				"http://192.168.0.103:8000"
			};

			var uris = uriStrings
				.Select(uriString => new Uri(uriString))
				.ToArray();

			using (var host = new NancyHost(hostConfiguration, uris))
			{
				host.Start();

				Console.WriteLine("Nancy now listening on:");
				foreach (var uriString in uriStrings) {
					Console.WriteLine(uriString);
				}
				Console.WriteLine("Press ENTER to quit");
				Console.ReadKey();
			}

			Console.WriteLine("Stopped. Good bye!");
		}
	}
}
