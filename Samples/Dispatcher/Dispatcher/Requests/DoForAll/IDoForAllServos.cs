using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading;

namespace Dispatcher.Requests.Test
{
	[Serializable]
	abstract class IDoForAllServos : IRequest
	{
		public bool voiceEnabled { get; set; } = true;
		public float timeout { get; set; } = 20;
		Registers registers { get; set; } = null;
		public int accuracy { get; set; } = 10;

		protected abstract void Perform(DynamixelSDKSharp.Servo servo, Action<int> goToPosition, int startPosition);

		public object Perform()
		{
			var results = new Dictionary<int, object>();

			using (var speechSynthesizer = new SpeechSynthesizer())
			{
				speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);

				DynamixelSDKSharp.Servo servo = null;
				DateTime startServoTime = DateTime.Now;

				Action checkTimeout = () =>
				{
					var didTimeout = (DateTime.Now - startServoTime).TotalSeconds > this.timeout;
					if (didTimeout)
					{
						throw(new Exception(String.Format("Timeout nuding servo {0}", servo.ID)));
					}
				};

				var servos = PortPool.X.Servos;
				foreach (var servoKeyValue in servos)
				{
					try
					{
						servo = servoKeyValue.Value;

						//save time for timeout checks
						startServoTime = DateTime.Now;

						//speak the servo ID
						Prompt speechPrompt = null;
						if(this.voiceEnabled)
						{
							try
							{
								speechPrompt = speechSynthesizer.SpeakAsync("Servo " + servoKeyValue.Key.ToString());
							}
							catch
							{
								speechPrompt = null;
							}
						}

						//store values before we started
						var oldRegisters = new List<Register>();
						oldRegisters.AddRange(new[] { 
							servo.ReadRegister(RegisterType.TorqueEnable),
							servo.ReadRegister(RegisterType.ProfileVelocity),
							servo.ReadRegister(RegisterType.ProfileAcceleration),
							servo.ReadRegister(RegisterType.PositionIGain)
						});

						//start position
						var startPosition = servo.ReadValue(RegisterType.PresentPosition);

						//turn on LED
						servo.WriteValue(RegisterType.LED, 1);

						//set movement properties
						servo.WriteValue(RegisterType.TorqueEnable, 1);
						servo.WriteValue(RegisterType.ProfileVelocity, 20);
						servo.WriteValue(RegisterType.ProfileAcceleration, 2);
						servo.WriteValue(RegisterType.PositionIGain, 100);

						//set any custom registers
						if (this.registers != null)
						{
							foreach (var register in this.registers.Values)
							{
								//store old value
								//Note : these will enter the FIFO list after the registers above, so will overwrite on the way outs
								oldRegisters.Add(register.Clone() as Register);

								//set new value
								servo.WriteValue(register);
							}
						}

						//function for moving to position
						Action<int> goToPosition = (int target) =>
						{

							//clamp target position to valid range
							if (target < 0)
							{
								target = 0;
							}
							if (target >= servo.ProductSpecification.EncoderResolution)
							{
								target = servo.ProductSpecification.EncoderResolution - 1;
							}

							servo.WriteValue(RegisterType.GoalPosition, target);

							while (Math.Abs(servo.ReadValue(RegisterType.PresentPosition) - target) > this.accuracy)
							{
								checkTimeout();
								Thread.Sleep(500);
							}
						};



						//----
						//Do the action
						this.Perform(servo, goToPosition, startPosition);
						//
						//----



						//reset registers
						foreach (var register in oldRegisters)
						{
							servo.WriteValue(register);
						}

						//wait for speech to finish
						if (speechPrompt != null)
						{
							while (!speechPrompt.IsCompleted)
							{
								checkTimeout();
								Thread.Sleep(10);
							}
						}

						//log success
						Logger.Log(Logger.Level.Trace, String.Format("Servo {0} success", servo.ID), this.GetType());

						//turn off LED if successful
						servo.WriteValue(RegisterType.LED, 0);

						//output success result
						results.Add(servo.ID, new
						{
							success = true
						});
					}
					catch (Exception e)
					{
						//log fail
						Logger.Log<NudgeAllServos>(Logger.Level.Error, e);

						//speak fail
						if(this.voiceEnabled)
						{
							speechSynthesizer.Speak(String.Format("Issue with servo {0}", servo.ID));
						}

						//output fail result
						results.Add(servo.ID, new
						{
							success = false,
							exception = new Utils.ExceptionMessage(e)
						});
					}
				}

				if(voiceEnabled)
				{
					speechSynthesizer.SpeakAsync("END");
				}
			}

			return results;
		}
	}
}