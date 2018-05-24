using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Requests
{
	public enum Method
	{
		GET,
		POST
	}

	public enum ThreadUsage
	{
		Shared,
		Exclusive
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class RequestHandlerAttribute : Attribute
	{
		public string CustomAddress { get; set; } = null;
		public Method Method { get; set; } = Method.GET;
		public ThreadUsage ThreadUsage { get; set; } = ThreadUsage.Shared;
	}

	interface IRequest
	{
		object Perform();
	}
}
