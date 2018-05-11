using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynamixelSDKSharp
{
	class WorkerThread
	{
		public delegate void Action();
		class SyncAction
		{
			public Action Action;
			public Exception Exception = null;
			public Object SyncLock = new object();
		};

		Thread Thread;
		bool IsJoining = false;

		Queue<Action> ActionQueue = new Queue<Action>();
		Queue<SyncAction> SyncActionQueue = new Queue<SyncAction>();

		private List<Exception> ExceptionsInThread = new List<Exception>();
		public List<Exception> GetExceptionsInThread(bool flushList = true)
		{
			lock (this.ExceptionsInThread)
			{
				var list = this.ExceptionsInThread;
				if (flushList)
				{
					this.ExceptionsInThread = new List<Exception>();
				}
				return list;
			}
		}

		readonly object SyncPrimitive = new object();

		public WorkerThread(string name)
		{
			this.Thread = new Thread(new ThreadStart(ThreadedFunction));
			this.Thread.Name = name;
			this.Thread.IsBackground = true;
			this.Thread.Start();
		}

		public void Do(Action Action)
		{
			//first check if we're already inside thread
			if (Thread.CurrentThread == this.Thread)
			{
				Action();
			}
			else
			{
				//add action to queue
				lock (this.ActionQueue)
				{
					this.ActionQueue.Enqueue(Action);
				}

				//notify thread
				lock (this.SyncPrimitive)
				{
					Monitor.Pulse(this.SyncPrimitive);
				}
			}

		}

		public void DoSync(Action Action, TimeSpan timeout)
		{
			//first check if we're already inside thread
			if (Thread.CurrentThread == this.Thread)
			{
				Action();
			}
			else
			{
				var syncAction = new SyncAction
				{
					Action = Action
				};

				//add action to queue
				lock (this.SyncActionQueue)
				{
					this.SyncActionQueue.Enqueue(syncAction);
				}

				//notify thread
				lock (this.SyncPrimitive)
				{
					Monitor.Pulse(this.SyncPrimitive);
				}

				//wait for a response
				lock (syncAction.SyncLock)
				{
					if (timeout == TimeSpan.Zero)
					{
						Monitor.Wait(syncAction.SyncLock);
					}
					else
					{
						Monitor.Wait(syncAction.SyncLock, timeout);
					}
				}

				//throw exception if any
				if (syncAction.Exception != null)
				{
					throw (syncAction.Exception);
				}
			}
		}

		public void DoSync(Action Action)
		{
			DoSync(Action, TimeSpan.Zero);
		}

		private void ThreadedFunction()
		{

			//loop whilst thread is running
			while (!this.IsJoining)
			{
				// ACTION QUEUE
				{
					//loop whilst there are actions in the queue (before waiting on the monitor)
					while (true)
					{
						Action action = null;
						lock (this.ActionQueue)
						{
							if (this.ActionQueue.Count > 0)
							{
								action = this.ActionQueue.Dequeue();
							}
						}
						if (action != null)
						{
							try
							{
								action();
							}
							catch (Exception e)
							{
								this.ExceptionsInThread.Add(e);
							}
						}
						else
						{
							break;
						}
					}
				}

				// SYNC ACTION QUEUE
				{
					//loop whilst there are actions in the queue (before waiting on the monitor)
					while (true)
					{
						SyncAction action = null;
						lock (this.SyncActionQueue)
						{
							if (this.SyncActionQueue.Count > 0)
							{
								action = this.SyncActionQueue.Dequeue();
							}
						}
						if (action != null)
						{
							try
							{
								action.Action();
							}
							catch (Exception e)
							{
								action.Exception = e;
							}
							lock (action.SyncLock)
							{
								Monitor.Pulse(action.SyncLock);
							}
						}
						else
						{
							break;
						}
					}
				}

				//wait for notification of next action
				lock (this.SyncPrimitive)
				{
					Monitor.Wait(SyncPrimitive, TimeSpan.FromMilliseconds(10));
				}
			}
		}

		public void Join()
		{
			this.IsJoining = true;
			lock (this.SyncPrimitive)
			{
				Monitor.Pulse(this.SyncPrimitive);
			}
			this.Thread.Join();
		}
	}
}
