﻿using System;
using System.Collections.Generic;




namespace RI.Framework.StateMachines
{
	/// <summary>
	/// Implements a default state cache suitable for most scenarios.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="StateCache"/> internally uses a simple dictionary with the state type as key.
	/// </para>
	/// <para>
	/// See <see cref="IStateCache"/> for more details.
	/// </para>
	/// </remarks>
	public sealed class StateCache : IStateCache
	{
		/// <summary>
		/// Creates a new instance of <see cref="StateCache"/>.
		/// </summary>
		public StateCache ()
		{
			this.States = new Dictionary<Type, IState>();
		}

		/// <summary>
		/// Gets the dictionary with all cached state instances.
		/// </summary>
		/// <value>
		/// The dictionary with all cached state instances.
		/// </value>
		public Dictionary<Type, IState> States { get; private set; }

		/// <inheritdoc />
		public bool ContainsState (Type state)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			return this.States.ContainsKey(state);
		}

		/// <inheritdoc />
		public IState GetState (Type state)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			if (!this.States.ContainsKey(state))
			{
				throw new KeyNotFoundException();
			}

			return this.States[state];
		}

		/// <inheritdoc />
		public void AddState (IState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			Type type = state.GetType();
			if (this.States.ContainsKey(type))
			{
				this.States[type] = state;
			}
			else
			{
				this.States.Add(type, state);
			}
		}

		/// <inheritdoc />
		public void RemoveState (IState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			Type type = state.GetType();
			this.States.Remove(type);
		}

		/// <inheritdoc />
		public void Clear ()
		{
			this.States.Clear();
		}
	}
}