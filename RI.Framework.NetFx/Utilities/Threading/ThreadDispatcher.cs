﻿using System;
using System.Threading;
using System.Threading.Tasks;

using RI.Framework.Collections.Generic;
using RI.Framework.Utilities.ObjectModel;




namespace RI.Framework.Utilities.Threading
{
	/// <summary>
	///     Implements a delegate dispatcher which can be run on any thread.
	/// </summary>
	/// <remarks>
	///     <para>
	///         A <see cref="ThreadDispatcher" /> provides a queue for delegates, filled through <see cref="Send(Delegate,object[])" />/<see cref="Send(int,Delegate,object[])"/> and <see cref="Post(Delegate,object[])" />/<see cref="Post(int,System.Delegate,object[])"/>, which is processed on the thread where <see cref="Run" /> is called (<see cref="Run" /> blocks while executing the queue until <see cref="Shutdown" /> is called).
	///     </para>
	///     <para>
	///         The delegates are executed in the order they are added to the queue through <see cref="Send(Delegate,object[])" />/<see cref="Send(int,Delegate,object[])"/> or <see cref="Post(Delegate,object[])" />/<see cref="Post(int,System.Delegate,object[])"/>.
	///         When all delegates are executed, or the queue is empty respectively, <see cref="ThreadDispatcher" /> waits for new delegates to process.
	///     </para>
	///     <para>
	///         During <see cref="Run" />, the current <see cref="SynchronizationContext" /> is replaced by an instance of <see cref="ThreadDispatcherSynchronizationContext" /> and restored afterwards.
	///     </para>
	/// </remarks>
	public sealed class ThreadDispatcher : IThreadDispatcher, ISynchronizable
	{
		#region Instance Constructor/Destructor

		/// <summary>
		///     Creates a new instance of <see cref="ThreadDispatcher" />.
		/// </summary>
		public ThreadDispatcher ()
		{
			this.SyncRoot = new object();

			this.Thread = null;
			this.Queue = null;
			this.Posted = null;

			this.PreRunQueue = new PriorityQueue<ThreadDispatcherOperation>();

			this.ShutdownMode = ThreadDispatcherShutdownMode.None;

			this.DefaultPriority = ThreadDispatcher.DefaultPriorityValue;
			this.CatchExceptions = false;
		}

		/// <summary>
		///     Garbage collects this instance of <see cref="ThreadDispatcher" />.
		/// </summary>
		~ThreadDispatcher ()
		{
			this.Posted?.Close();
			this.Posted = null;

			this.Queue?.Clear();
			this.Queue = null;

			this.PreRunQueue?.Clear();
			this.PreRunQueue = null;
		}

		#endregion



		/// <summary>
		/// Gets the default value for <see cref="DefaultPriority"/> if it is not explicitly set.
		/// </summary>
		/// <value>
		/// The default value for <see cref="DefaultPriority"/> if it is not explicitly set.
		/// </value>
		public const int DefaultPriorityValue = int.MaxValue / 2;




		#region Instance Fields

		private int _defaultPriority;
		private bool _catchExceptions;
		private ThreadDispatcherShutdownMode _shutdownMode;

		#endregion




		#region Instance Properties/Indexer

		private ThreadDispatcherOperation OperationInProgress { get; set; }
		private ManualResetEvent Posted { get; set; }
		private PriorityQueue<ThreadDispatcherOperation> PreRunQueue { get; set; }
		private PriorityQueue<ThreadDispatcherOperation> Queue { get; set; }

		private object SyncRoot { get; set; }
		private Thread Thread { get; set; }

		#endregion




		#region Instance Methods

		/// <summary>
		///     Processes the delegate queue or waits for new delegates until <see cref="Shutdown" /> is called.
		/// </summary>
		/// <exception cref="InvalidOperationException"> The dispatcher is already running. </exception>
		/// <exception cref="ThreadDispatcherException"> The execution of a delegate has thrown an exception and <see cref="CatchExceptions"/> is false. </exception>
		public void Run ()
		{
			SynchronizationContext synchronizationContextBackup = SynchronizationContext.Current;

			try
			{
				lock (this.SyncRoot)
				{
					synchronizationContextBackup = SynchronizationContext.Current;

					this.VerifyNotRunning();

					this.Thread = Thread.CurrentThread;
					this.Queue = new PriorityQueue<ThreadDispatcherOperation>();
					this.Posted = new ManualResetEvent(this.PreRunQueue.Count > 0);
					this.ShutdownMode = ThreadDispatcherShutdownMode.None;

					this.PreRunQueue.MoveTo(this.Queue);

					SynchronizationContext.SetSynchronizationContext(new ThreadDispatcherSynchronizationContext(this));
				}

				this.ExecuteFrame(null);
			}
			finally
			{
				lock (this.SyncRoot)
				{
					SynchronizationContext.SetSynchronizationContext(synchronizationContextBackup);

					this.Posted?.Close();
					this.Queue?.Clear();

					this.Posted = null;
					this.Queue = null;
					this.Thread = null;
				}
			}
		}

		private void ExecuteFrame (ThreadDispatcherOperation returnTrigger)
		{
			while (true)
			{
				this.Posted.WaitOne();

				lock (this.SyncRoot)
				{
					this.Posted.Reset();
				}

				while (true)
				{
					ThreadDispatcherOperation operation = null;

					lock (this.SyncRoot)
					{
						this.OperationInProgress = null;

						if (this.ShutdownMode == ThreadDispatcherShutdownMode.DiscardPending)
						{
							foreach (ThreadDispatcherOperation operationToCancel in this.Queue)
							{
								if (operationToCancel.State == ThreadDispatcherOperationState.Waiting)
								{
									operationToCancel.Cancel();
								}
							}
							this.Queue.Clear();
							return;
						}

						if ((this.ShutdownMode == ThreadDispatcherShutdownMode.FinishPending) && (this.Queue.Count == 0))
						{
							return;
						}

						if (this.Queue.Count > 0)
						{
							operation = this.Queue.Dequeue();
							this.OperationInProgress = operation;
						}
					}

					if (operation == null)
					{
						break;
					}

					operation.Execute();

					lock (this.SyncRoot)
					{
						this.OperationInProgress = null;
					}

					if (operation.State == ThreadDispatcherOperationState.Exception)
					{
						bool canContinue = this.CatchExceptions;

						this.OnException(operation.Exception, canContinue);

						if (!canContinue)
						{
							throw new ThreadDispatcherException(operation.Exception);
						}
					}

					if (object.ReferenceEquals(operation, returnTrigger))
					{
						return;
					}
				}
			}
		}

		private void OnException (Exception exception, bool canContinue)
		{
			this.Exception?.Invoke(this, new ThreadDispatcherExceptionEventArgs(exception, canContinue));
		}

		private void VerifyNotRunning ()
		{
			if (this.IsRunning)
			{
				throw new InvalidOperationException(nameof(ThreadDispatcher) + " is already running.");
			}
		}

		private void VerifyNotShuttingDown ()
		{
			if (this.ShutdownMode != ThreadDispatcherShutdownMode.None)
			{
				throw new InvalidOperationException(nameof(ThreadDispatcher) + " is already shutting down.");
			}
		}

		private void VerifyRunning ()
		{
			if (!this.IsRunning)
			{
				throw new InvalidOperationException(nameof(ThreadDispatcher) + " is not running.");
			}
		}

		private void VerifyShuttingDown ()
		{
			if (this.ShutdownMode == ThreadDispatcherShutdownMode.None)
			{
				throw new InvalidOperationException(nameof(ThreadDispatcher) + " is not shutting down.");
			}
		}

		#endregion




		#region Interface: IThreadDispatcher

		/// <inheritdoc />
		public bool CatchExceptions
		{
			get
			{
				lock (this.SyncRoot)
				{
					return this._catchExceptions;
				}
			}
			set
			{
				lock (this.SyncRoot)
				{
					this._catchExceptions = value;
				}
			}
		}

		/// <inheritdoc />
		public bool IsRunning
		{
			get
			{
				lock (this.SyncRoot)
				{
					return this.Thread != null;
				}
			}
		}

		/// <inheritdoc />
		public bool IsShuttingDown => this.ShutdownMode != ThreadDispatcherShutdownMode.None;

		/// <inheritdoc />
		bool ISynchronizable.IsSynchronized => true;

		/// <inheritdoc />
		public ThreadDispatcherShutdownMode ShutdownMode
		{
			get
			{
				lock (this.SyncRoot)
				{
					if (!this.IsRunning)
					{
						return ThreadDispatcherShutdownMode.None;
					}

					return this._shutdownMode;
				}
			}
			private set
			{
				lock (this.SyncRoot)
				{
					this._shutdownMode = value;
				}
			}
		}

		/// <inheritdoc />
		public int DefaultPriority
		{
			get
			{
				lock (this.SyncRoot)
				{
					return this._defaultPriority;
				}
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				lock (this.SyncRoot)
				{
					this._defaultPriority = value;
				}
			}
		}

		/// <inheritdoc />
		object ISynchronizable.SyncRoot => this.SyncRoot;

		/// <inheritdoc />
		public event EventHandler<ThreadDispatcherExceptionEventArgs> Exception;

		/// <inheritdoc />
		public void DoProcessing ()
		{
			while (true)
			{
				this.Send(0, new Action(() => { }));

				lock (this.SyncRoot)
				{
					if ((this.Queue.Count == 0) && (this.OperationInProgress == null))
					{
						return;
					}
				}
			}
		}

		/// <inheritdoc />
		public async Task DoProcessingAsync ()
		{
			while (true)
			{
				await this.SendAsync(0, new Action(() => { }));

				lock (this.SyncRoot)
				{
					if ((this.Queue.Count == 0) && (this.OperationInProgress == null))
					{
						return;
					}
				}
			}
		}

		/// <inheritdoc />
		public bool IsInThread ()
		{
			lock (this.SyncRoot)
			{
				if (this.Thread == null)
				{
					return false;
				}

				return this.Thread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId;
			}
		}

		/// <inheritdoc />
		public ThreadDispatcherOperation Post (Delegate action, params object[] parameters)
		{
			return this.Post(this.DefaultPriority, action, parameters);
		}

		/// <inheritdoc />
		public ThreadDispatcherOperation Post (int priority, Delegate action, params object[] parameters)
		{
			if (priority < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(priority));
			}

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			parameters = parameters ?? new object[0];

			lock (this.SyncRoot)
			{
				this.VerifyNotShuttingDown();

				ThreadDispatcherOperation operation = new ThreadDispatcherOperation(this, action, parameters);

				if (this.IsRunning)
				{
					this.Queue.Enqueue(operation, priority);
					this.Posted.Set();
				}
				else
				{
					this.PreRunQueue.Enqueue(operation, priority);
				}

				return operation;
			}
		}

		/// <inheritdoc />
		public object Send (Delegate action, params object[] parameters)
		{
			return this.Send(this.DefaultPriority, action, parameters);
		}

		/// <inheritdoc />
		public object Send (int priority, Delegate action, params object[] parameters)
		{
			if (priority < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(priority));
			}

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			parameters = parameters ?? new object[0];

			bool isInThread;
			ThreadDispatcherOperation operation;

			lock (this.SyncRoot)
			{
				this.VerifyRunning();
				this.VerifyNotShuttingDown();

				isInThread = this.IsInThread();
				operation = this.Post(priority, action, parameters);
			}

			if (isInThread)
			{
				this.ExecuteFrame(operation);
			}
			else
			{
				operation.Wait();
			}

			if (operation.Exception != null)
			{
				throw new ThreadDispatcherException(operation.Exception);
			}

			return operation.Result;
		}

		/// <inheritdoc />
		public async Task<object> SendAsync (Delegate action, params object[] parameters)
		{
			return await this.SendAsync(this.DefaultPriority, action, parameters);
		}

		/// <inheritdoc />
		public async Task<object> SendAsync (int priority, Delegate action, params object[] parameters)
		{
			if (priority < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(priority));
			}

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			parameters = parameters ?? new object[0];

			ThreadDispatcherOperation operation;

			lock (this.SyncRoot)
			{
				this.VerifyRunning();
				this.VerifyNotShuttingDown();

				operation = this.Post(priority, action, parameters);
			}

			await operation.WaitAsync();

			return operation.Result;
		}

		/// <inheritdoc />
		public void Shutdown (bool finishPendingDelegates)
		{
			lock (this.SyncRoot)
			{
				this.VerifyRunning();
				this.VerifyNotShuttingDown();

				this.ShutdownMode = finishPendingDelegates ? ThreadDispatcherShutdownMode.FinishPending : ThreadDispatcherShutdownMode.DiscardPending;
				this.Posted.Set();
			}
		}

		#endregion
	}
}
