﻿using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DynamixelREST
{
	namespace Safety
	{
		public class Constraints : List<Constraint>
		{
			//Singleton
			public static readonly Constraints X = new Constraints();

			public const string Filename = "./Safety/Constraints.json";

			Constraints()
			{
				try
				{
					this.Load(Constraints.Filename);
				}
				catch(Exception e)
				{
					Logger.Log<Constraints>(Logger.Level.Error, e);
				}
			}

			void Load(string filename)
			{
				//load list of constraints
				var jsonMapping = new
				{
					SafetyConstraints = this
				};
				using (StreamReader file = new StreamReader(filename))
				{
					var json = file.ReadToEnd();
					JsonConvert.PopulateObject(json, jsonMapping, ProductDatabase.JsonSerializerSettings);
				}
			}
		}

		[Serializable]
		public class Constraint
		{
			public enum ActionType
			{
				None,
				ShutdownOne,
				ShutdownAll,
				ClampValue,
				MiddleValue,
				TakeARest,
				LimitCurrent
			}

			public RegisterType RegisterType { get; set; }
			public int MinValue { get; set; }
			public int MaxValue { get; set; }
			public ActionType Action { get; set; }
			public double Period { get; set; } = 1;
			public Logger.Level LogLevel { get; set; } = Logger.Level.Warning;

			public Dictionary<int, DateTime> LastPerformTime { get; private set; } = new Dictionary<int, DateTime>();

			public bool NeedsPerform(Servo servo)
			{
				if (this.LastPerformTime.ContainsKey(servo.ID))
				{
					//Check if performed recently
					return (DateTime.Now - LastPerformTime[servo.ID]).TotalSeconds > this.Period;
				}
				else
				{
					//It has never been performed
					return true;
				}
			}

			public void Perform(Servo servo)
			{
				var value = servo.ReadValue(this.RegisterType);

				bool isTooLow = value < this.MinValue;
				bool isTooHigh = value > this.MaxValue;

				if (isTooLow || isTooHigh)
				{
					//value is outside of range

					//always log
					Logger.Log<Constraints>(this.LogLevel, String.Format("Servo {0} register {1} has value {2} which is out of range ({3}->{4}). Performing action {5}."
						, servo.ID
						, this.RegisterType.ToString()
						, value
						, this.MinValue
						, this.MaxValue
						, this.Action.ToString()));

					switch (this.Action)
					{
						case ActionType.ShutdownOne:
							servo.WriteValue(RegisterType.TorqueEnable, 0);
							break;
						case ActionType.ShutdownAll:
							PortPool.X.ShutdownAll();
							break;
						case ActionType.ClampValue:
							if (isTooLow)
							{
								servo.WriteValue(this.RegisterType, this.MinValue);
							}
							else
							{
								servo.WriteValue(this.RegisterType, this.MaxValue);
							}
							break;
						case ActionType.MiddleValue:
							{
								var middleValue = (this.MaxValue - this.MinValue) / 2 + this.MinValue;
								servo.WriteValue(this.RegisterType, middleValue);
							}
							break;
						case ActionType.TakeARest:
							{
								ServoRestManager.X.PutToRest(servo);
							}
							break;
						case ActionType.LimitCurrent:
							{
								var torqueWasEnabled = servo.ReadValue(RegisterType.TorqueEnable);
								var previousGoalPosition = servo.ReadValue(RegisterType.GoalPosition);

								servo.WriteValue(RegisterType.TorqueEnable, 0);
								{
									servo.WriteValue(RegisterType.CurrentLimit, 100);
								}

								servo.WriteValue(RegisterType.TorqueEnable, torqueWasEnabled);
								servo.WriteValue(RegisterType.GoalPosition, previousGoalPosition);
							}
							break;
						default:
							break;
					}
				}

				if(!this.LastPerformTime.ContainsKey(servo.ID))
				{
					this.LastPerformTime.Add(servo.ID, DateTime.Now);
				}
				else
				{
					this.LastPerformTime[servo.ID] = DateTime.Now;
				}
			}
		}
	}
	
}
