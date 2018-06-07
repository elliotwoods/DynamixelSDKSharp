using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dispatcher.Requests.Group
{
	[RequestHandler(Method = Method.POST)]
	class MoveAndLogAll : IRequest
	{
		public RegisterType register { get; set; } = RegisterType.GoalPosition;
		public List<int> positions { get; set; }
		public bool useCachedDatum { get; set; } = true;

		public double delayBeforeLog { get; set; } = 20;
		public RegisterType logRegister { get; set; } = RegisterType.PresentPosition;

		static Dictionary<int, int> SCachedDatum = new Dictionary<int, int>();

		public object Perform()
		{
			using (var speechSynthesizer = new SpeechSynthesizer())
			{
				var requestTime = DateTime.Now;

				var servos = PortPool.X.Servos;

				if (this.positions == null || this.positions.Count == 0)
				{
					throw (new Exception("No datum set"));
				}

				if (!this.useCachedDatum)
				{
					MoveAndLogAll.SCachedDatum.Clear();
				}

				speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);

				speechSynthesizer.SpeakAsync("Moving and logging positions by datum");

				if (!Database.Connection.X.Connected)
				{
					speechSynthesizer.SpeakAsync("Database connection fail");
					throw (new Exception("Database connection fail"));
				}

				foreach (var position in this.positions)
				{
					speechSynthesizer.SpeakAsync(position.ToString());
				}


				//build the targets
				var targets = new Dictionary<int, int>();
				foreach (var servoIterator in servos)
				{
					int datum;
					//check if no cached datum is available
					if (!MoveAndLogAll.SCachedDatum.ContainsKey(servoIterator.Key))
					{
						//no datum cached

						//calc datum
						var max = servoIterator.Value.ReadValue(RegisterType.MaxPositionLimit);
						var min = servoIterator.Value.ReadValue(RegisterType.MinPositionLimit);
						datum = (max + min) / 2;

						MoveAndLogAll.SCachedDatum.Add(servoIterator.Key, datum);
					}
					else
					{
						datum = MoveAndLogAll.SCachedDatum[servoIterator.Key];
					}

					var target = datum + this.positions[servoIterator.Key % this.positions.Count];

					targets.Add(servoIterator.Key, target);
				}

				//move to targets
				foreach (var target in targets)
				{
					servos[target.Key].WriteValue(this.register, target.Value);
				}

				//wait before start logging
				Thread.Sleep(TimeSpan.FromSeconds(this.delayBeforeLog));

				//perform the log
				var collection = Database.Connection.X.GetCollection<Database.Registers>();
				foreach (var servo in servos)
				{
					var servoID = servo.Key;

					var valuesForServo = new Dictionary<string, int>();
					valuesForServo.Add(this.logRegister.ToString()
							, servo.Value.ReadValue(this.logRegister));

					var row = new Database.Registers
					{
						ServoID = servoID,
						TimeStamp = requestTime,
						RegisterValues = valuesForServo
					};

					collection.InsertOne(row);
				}

				//if final one is async we'll dispose before it's finished
				speechSynthesizer.Speak(String.Format("Logging complete for {0} servos", servos.Count));

				return new { };
			}
		}
	}
}
