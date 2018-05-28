using Nancy;
using Nancy.ModelBinding;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DynamixelSDKSharp;
using Nancy.Responses;
using System.Reflection;

namespace Dispatcher
{
	public class Module : NancyModule
	{
		public Module()
		{
			//Module 
			
			//We don't use this any more. But this might be useful for debugging Nancy later
		}
	}
}
