using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Requests.Group
{
	[RequestHandler(Method = Method.POST | Method.GET)]
	class LogRegister : IRequest
	{
		public object Perform()
		{
			return new { };
		}
	}
}
