﻿using System;

using RI.Framework.Composition.Model;
using RI.Framework.Services.Logging;

using UnityEngine;

using Object = UnityEngine.Object;




namespace RI.Framework.Services.Modularization
{
	/// <summary>
	///     Implements a base class which can be used for <c> MonoBehaviour </c> based module implementation.
	/// </summary>
	/// <remarks>
	///     <note type="note">
	///         Instances of <see cref="MonoModule" />s are not created using their constructor (as this would be the wrong way how to instantiate anything <c> MonoBehaviour </c>). Instead, <see cref="CreateInstance" /> is used.
	///     </note>
	/// </remarks>
	[Export]
	public abstract class MonoModule : MonoBehaviour,
	                                   IModule
	{
		#region Static Methods

		/// <summary>
		///     Creates an instance of the specified <see cref="MonoModule" /> type.
		/// </summary>
		/// <param name="type"> The type of which an instance is to be created. </param>
		/// <returns> The created instance. </returns>
		/// <remarks>
		///     <para>
		///         To instantiate a <see cref="MonoModule" />, a new <c> GameObject </c> is created to which the <see cref="MonoModule" /> is added as a component using <c> AddComponent </c>.
		///         The created <c> GameObject </c> has also called <c> Object.DontDestroyOnLoad </c> on it.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="type" /> is null. </exception>
		[ExportCreator]
		public static MonoModule CreateInstance (Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			GameObject gameObject = new GameObject();
			MonoModule instance = gameObject.AddComponent(type) as MonoModule;
			Object.DontDestroyOnLoad(gameObject);
			return instance;
		}

		#endregion




		#region Instance Methods

		/// <summary>
		///     Logs a message.
		/// </summary>
		/// <param name="severity"> The severity of the message. </param>
		/// <param name="format"> The message. </param>
		/// <param name="args"> The arguments which will be expanded into the message (comparable to <see cref="string.Format(string, object[])" />). </param>
		/// <remarks>
		///     <para>
		///         <see cref="ILogService" /> is used, obtained through <see cref="LogLocator" />.
		///         If no <see cref="ILogService" /> is available, no logging is performed.
		///     </para>
		/// </remarks>
		protected void Log (LogLevel severity, string format, params object[] args)
		{
			LogLocator.Log(severity, this.GetType().Name, format, args);
		}

		#endregion




		#region Virtuals

		/// <inheritdoc cref="IModule.Initialize" />
		protected virtual void Initialize ()
		{
		}

		/// <inheritdoc cref="IModule.Unload" />
		protected virtual void Unload ()
		{
		}

		#endregion




		#region Interface: IModule

		/// <inheritdoc />
		public bool IsInitialized { get; private set; }

		/// <inheritdoc />
		void IModule.Initialize ()
		{
			this.Log(LogLevel.Debug, "Initializing module");

			this.Initialize();

			this.IsInitialized = true;
		}

		/// <inheritdoc />
		void IModule.Unload ()
		{
			this.Log(LogLevel.Debug, "Unloading module");

			this.Unload();

			this.IsInitialized = false;
		}

		#endregion
	}
}
