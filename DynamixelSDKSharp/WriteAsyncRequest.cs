using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelSDKSharp
{
	public struct WriteAsyncRequest
	{
		public Servo servo;
		public RegisterType registerType;
	}
}
