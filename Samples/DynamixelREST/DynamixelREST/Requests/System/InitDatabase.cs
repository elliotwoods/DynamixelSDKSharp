﻿using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace DynamixelREST.Requests.System
{
	class InitDatabase : IRequest
	{
		public object Perform()
		{
			Database.Connection.X.Connect();
			return new { };
		}
	}
}
