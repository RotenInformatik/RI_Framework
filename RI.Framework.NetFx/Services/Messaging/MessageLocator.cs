﻿using System.Collections.Generic;

using RI.Framework.Services.Messaging.Dispatchers;




namespace RI.Framework.Services.Messaging
{
	/// <summary>
	///     Provides a centralized and global messaging provider.
	/// </summary>
	/// <remarks>
	///     <para>
	///         <see cref="MessageLocator" /> is merely a convenience utility as it uses <see cref="ServiceLocator" /> to retrieve and use a <see cref="IMessageService" />.
	///     </para>
	/// </remarks>
	public static class MessageLocator
	{
		/// <summary>
		///     Gets whether a messaging service is available and can be used by <see cref="MessageLocator" />.
		/// </summary>
		/// <value>
		///     true if a messaging service is available and can be used by <see cref="MessageLocator" />, false otherwise.
		/// </value>
		public static bool IsAvailable => MessageLocator.Service != null;

		/// <summary>
		///     Gets the available messaging service.
		/// </summary>
		/// <value>
		///     The messaging service or null if no messaging service is available.
		/// </value>
		public static IMessageService Service => ServiceLocator.GetInstance<IMessageService>();

		/// <inheritdoc cref="IMessageService.Dispatchers" />
		public static IEnumerable<IMessageDispatcher> Dispatchers => MessageLocator.Service?.Dispatchers ?? new IMessageDispatcher[0];

		/// <inheritdoc cref="IMessageService.Receivers" />
		public static IEnumerable<IMessageReceiver> Receivers => MessageLocator.Service?.Receivers ?? new IMessageReceiver[0];

		/// <inheritdoc cref="IMessageService.Post" />
		public static void Post(IMessage message) => MessageLocator.Service?.Post(message);
	}
}