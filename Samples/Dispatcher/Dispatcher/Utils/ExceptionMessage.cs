using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.Utils
{
	[Serializable]
	class ExceptionMessage
	{
		public string message { get; set; }
		public List<string> stackTrace { get; set; }
		public string source { get; set; }

		public ExceptionMessage(Exception e)
		{
			this.message = e.Message;
			this.stackTrace = e.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
			this.source = e.Source;
		}
	}
}
