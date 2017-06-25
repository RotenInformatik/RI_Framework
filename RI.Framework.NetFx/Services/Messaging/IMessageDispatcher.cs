﻿using System;
using System.Collections.Generic;

using RI.Framework.Composition.Model;
using RI.Framework.Utilities.ObjectModel;




namespace RI.Framework.Services.Messaging
{
	/// <summary>
	///     Defines the interface for a message dispatcher.
	/// </summary>
	/// <remarks>
	///     A message dispatcher is used by a <see cref="IMessageService" /> to actually enqueue and deliver the messages to the receivers.
	/// </remarks>
	/// <threadsafety static="true" instance="true" />
	[Export]
	public interface IMessageDispatcher : ISynchronizable
	{
		/// <summary>
		///     Asynchronously delivers a message.
		/// </summary>
		/// <param name="receivers"> The sequence of receivers. </param>
		/// <param name="message"> The message to deliver. </param>
		/// <param name="messageService"> The message service used to deliver the message. </param>
		/// <remarks>
		///     <note type="implement">
		///         This method must not return until the message is delivered to all receivers.
		///     </note>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="receivers" />, <paramref name="message" />, or <paramref name="messageService" /> is null. </exception>
		void Post (IEnumerable<IMessageReceiver> receivers, IMessage message, IMessageService messageService);
	}
}
