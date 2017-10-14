﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RI.Framework.Bus.Exceptions;
using RI.Framework.Utilities;
using RI.Framework.Utilities.Exceptions;
using RI.Framework.Utilities.ObjectModel;




namespace RI.Framework.Bus
{
	/// <summary>
	///     Represents a send operation.
	/// </summary>
	/// <threadsafety static="true" instance="true" />
	public sealed class SendOperation : ISynchronizable
	{
		#region Instance Constructor/Destructor

		/// <summary>
		///     Creates a new instance of <see cref="SendOperation" />.
		/// </summary>
		/// <param name="bus"> The bus to be associated with this send operation. </param>
		/// <exception cref="ArgumentNullException"> <paramref name="bus" /> is null. </exception>
		public SendOperation (IBus bus)
		{
			if (bus == null)
			{
				throw new ArgumentNullException(nameof(bus));
			}

			this.SyncRoot = new object();
			this.Bus = bus;

			this.Address = null;
			this.Global = null;
			this.Payload = null;
			this.Timeout = null;
			this.CancellationToken = null;

			this.IsBroadcast = false;
			this.IsProcessed = false;

			this.Result = null;
		}

		#endregion




		#region Instance Properties/Indexer

		/// <summary>
		///     Gets the address the message is sent to.
		/// </summary>
		/// <value>
		///     The address the message is sent to or null if no address is used.
		/// </value>
		public string Address { get; private set; }

		/// <summary>
		///     Gets the bus this send operation is associated with.
		/// </summary>
		/// <value>
		///     The bus this send operation is associated with.
		/// </value>
		public IBus Bus { get; }

		/// <summary>
		///     Gets the cancellation token which can be used to cancel the wait for responses or the collection of responses.
		/// </summary>
		/// <value>
		///     tHE cancellation token which can be used to cancel the wait for responses or the collection of responses.
		/// </value>
		public CancellationToken? CancellationToken { get; private set; }

		/// <summary>
		///     Gets whether the message is sent globally.
		/// </summary>
		/// <value>
		///     true if the message is sent globally, false if locally, null if not defined where the default value of the associated bus is used.
		/// </value>
		public bool? Global { get; private set; }

		/// <summary>
		///     Gets whether the message is sent as a broadcast.
		/// </summary>
		/// <value>
		///     true if the message is sent as broadcast, false otherwise.
		/// </value>
		public bool IsBroadcast { get; private set; }

		/// <summary>
		///     Gets whether this send operation is being processed.
		/// </summary>
		/// <value>
		///     true if the send operation is being processed, false otherwise.
		/// </value>
		public bool IsProcessed { get; private set; }

		/// <summary>
		///     Gets the payload of the message.
		/// </summary>
		/// <value>
		///     The payload of the message or null if no payload is used.
		/// </value>
		public object Payload { get; private set; }

		/// <summary>
		///     Gets the timeout for the message (waiting for or collecting responses).
		/// </summary>
		/// <value>
		///     The timeout for the message or null if not defined where the default value of the associated bus is used.
		/// </value>
		public TimeSpan? Timeout { get; private set; }

		private object Result { get; set; }

		#endregion




		#region Instance Methods

		/// <summary>
		///     Broadcasts the message to multiple receivers without responses.
		/// </summary>
		/// <returns>
		///     The task used to wait until the timeout for completing round-trips expired.
		///     The tasks result is the number of receivers which acknowledged the message.
		/// </returns>
		/// <remarks>
		///     <note type="important">
		///         <see cref="AsBroadcast" /> does not throw <see cref="BusResponseTimeoutException" /> for not responding receivers.
		///     </note>
		/// </remarks>
		/// <exception cref="InvalidOperationException"> The message is already being processed or the bus is not started. </exception>
		/// <exception cref="BusProcessingPipelineException"> The bus processing pipeline encountered an exception. </exception>
		/// <exception cref="BusConnectionBrokenException"> A used connection to a remote bus is broken. </exception>
		public async Task<int> AsBroadcast ()
		{
			Task<object> task;
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.IsProcessed = true;
				this.IsBroadcast = true;
				task = this.Bus.Enqueue(this);
			}

			this.Result = await task.ConfigureAwait(false);
			return ((ICollection)this.Result).Count;
		}

		/// <summary>
		///     Broadcasts the message to multiple receivers expecting a response from each receiver.
		/// </summary>
		/// <typeparam name="TResponse"> The type of the expected responses. </typeparam>
		/// <returns>
		///     The task used to wait until the timeout for completing round-trips expired.
		///     The tasks result is the list of responses.
		/// </returns>
		/// <remarks>
		///     <note type="important">
		///         <see cref="AsBroadcast{TResponse}" /> does not throw <see cref="BusResponseTimeoutException" /> for not responding receivers.
		///     </note>
		/// </remarks>
		/// <exception cref="InvalidOperationException"> The message is already being processed or the bus is not started. </exception>
		/// <exception cref="BusProcessingPipelineException"> The bus processing pipeline encountered an exception. </exception>
		/// <exception cref="BusConnectionBrokenException"> A used connection to a remote bus is broken. </exception>
		/// <exception cref="InvalidCastException"> The responses could not be casted to type <typeparamref name="TResponse" />. </exception>
		public async Task<List<TResponse>> AsBroadcast <TResponse> ()
		{
			Task<object> task;
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.IsProcessed = true;
				this.IsBroadcast = true;
				task = this.Bus.Enqueue(this);
			}

			this.Result = await task.ConfigureAwait(false);
			return ((ICollection)this.Result).Cast<TResponse>().ToList();
		}

		/// <summary>
		///     Sends the message to a single receiver without a response.
		/// </summary>
		/// <returns>
		///     The task used to wait until the round-trip completed.
		/// </returns>
		/// <exception cref="InvalidOperationException"> The message is already being processed or the bus is not started. </exception>
		/// <exception cref="BusProcessingPipelineException"> The bus processing pipeline encountered an exception. </exception>
		/// <exception cref="BusResponseTimeoutException"> The intended receiver did not respond within the used timeout. </exception>
		/// <exception cref="BusConnectionBrokenException"> A used connection to a remote bus is broken. </exception>
		public async Task AsSingle ()
		{
			Task<object> task;
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.IsProcessed = true;
				this.IsBroadcast = false;
				task = this.Bus.Enqueue(this);
			}

			this.Result = await task.ConfigureAwait(false);
		}

		/// <summary>
		///     Sends the message to a single receiver expecting a response.
		/// </summary>
		/// <typeparam name="TResponse"> The type of the expected response. </typeparam>
		/// <returns>
		///     The task used to wait until the round-trip completed.
		///     The tasks result is the received response.
		/// </returns>
		/// <exception cref="InvalidOperationException"> The message is already being processed or the bus is not started. </exception>
		/// <exception cref="BusProcessingPipelineException"> The bus processing pipeline encountered an exception. </exception>
		/// <exception cref="BusResponseTimeoutException"> The intended receiver did not respond within the used timeout. </exception>
		/// <exception cref="BusConnectionBrokenException"> A used connection to a remote bus is broken. </exception>
		/// <exception cref="InvalidCastException"> The response could not be casted to type <typeparamref name="TResponse" />. </exception>
		public async Task<TResponse> AsSingle <TResponse> ()
		{
			Task<object> task;
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.IsProcessed = true;
				this.IsBroadcast = false;
				task = this.Bus.Enqueue(this);
			}

			this.Result = await task.ConfigureAwait(false);
			return (TResponse)this.Result;
		}

		/// <summary>
		///     Sets the address the message is sent to.
		/// </summary>
		/// <param name="address"> The address the message is sent to or null if no address is used. </param>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="EmptyStringArgumentException"> <paramref name="address" /> is an empty string. </exception>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation ToAddress (string address)
		{
			if (address != null)
			{
				if (address.IsNullOrEmptyOrWhitespace())
				{
					throw new EmptyStringArgumentException(nameof(address));
				}
			}

			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.Address = address;
				return this;
			}
		}

		/// <summary>
		///     Sets to use the default value of the associated bus whether to send the message globally.
		/// </summary>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation ToDefaultGlobal ()
		{
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.Global = null;
				return this;
			}
		}

		/// <summary>
		///     Sets the message to be sent globally.
		/// </summary>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation ToGlobal ()
		{
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.Global = true;
				return this;
			}
		}

		/// <summary>
		///     Sets the message to be sent locally or globally.
		/// </summary>
		/// <param name="sendGlobally"> Specifes whether the message should be sent globally (true) or locally (false). </param>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation ToGlobal (bool sendGlobally)
		{
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.Global = sendGlobally;
				return this;
			}
		}

		/// <summary>
		///     Sets the message to be sent locally.
		/// </summary>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation ToLocal ()
		{
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.Global = false;
				return this;
			}
		}

		/// <summary>
		///     Sets the cancellation token which can be used to cancel the wait for responses or the collection of responses.
		/// </summary>
		/// <param name="cancellationToken"> The cancellation token or null if cancellation is not used. </param>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation WithCancellation (CancellationToken? cancellationToken)
		{
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.CancellationToken = cancellationToken;
				return this;
			}
		}

		/// <summary>
		///     Sets to use the default timeoutfor the message (waiting for or collecting responses) of the associated bus.
		/// </summary>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation WithDefaultTimeout ()
		{
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.Timeout = null;
				return this;
			}
		}

		/// <summary>
		///     Sets the payload of the message.
		/// </summary>
		/// <param name="payload"> The payload of the message or null if no payload is used. </param>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation WithPayload (object payload)
		{
			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.Payload = payload;
				return this;
			}
		}

		/// <summary>
		///     Sets the timeout for the message (waiting for or collecting responses).
		/// </summary>
		/// <param name="timeout"> The timeout. </param>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException"> <paramref name="timeout" /> is negative. </exception>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation WithTimeout (TimeSpan timeout)
		{
			if (timeout.IsNegative())
			{
				throw new ArgumentOutOfRangeException(nameof(timeout));
			}

			lock (this.SyncRoot)
			{
				this.VerifyNotStarted();
				this.Timeout = timeout;
				return this;
			}
		}

		/// <summary>
		///     Sets the timeout for the message (waiting for or collecting responses).
		/// </summary>
		/// <param name="milliseconds"> The timeout in milliseconds. </param>
		/// <returns>
		///     The send operation to continue configuration of the message.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException"> <paramref name="milliseconds" /> is negative. </exception>
		/// <exception cref="InvalidOperationException"> The message is already being processed. </exception>
		public SendOperation WithTimeout (int milliseconds) => this.WithTimeout(TimeSpan.FromMilliseconds(milliseconds));

		private void VerifyNotStarted ()
		{
			if (this.IsProcessed)
			{
				throw new InvalidOperationException("The message is already being processed.");
			}
		}

		#endregion




		#region Interface: ISynchronizable

		/// <inheritdoc />
		bool ISynchronizable.IsSynchronized => true;

		/// <inheritdoc />
		public object SyncRoot { get; }

		#endregion
	}
}
