﻿using System;




namespace RI.Framework.StateMachines
{
	/// <summary>
	/// The actual state machine.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="StateMachine"/> is the actual state machine which manages the current state, signals, and state transitions.
	/// </para>
	/// <para>
	/// Multiple <see cref="StateMachine"/> instances can be used independently.
	/// However, it is important that they do not share certain aspects of their configuration.
	/// See <see cref="StateMachineConfiguration"/> for more details.
	/// </para>
	/// <para>
	/// A state machine has four core concepts: states, signals, transitions, and dispatching.
	/// </para>
	/// <para>
	/// STATES: A state machine always has a current state (<see cref="State"/>) which is either null or an instance of <see cref="IState"/>.
	/// The initial state after a <see cref="StateMachine"/> is created is always null (and is therefore usually used to expresss &quot;no state&quot; and/or to shut down the state machine by a transition to the null state).
	/// Instead of directly using instances of <see cref="IState"/> when working with state machines, you use the states <see cref="Type"/>.
	/// The state machine then resolves the actual <see cref="IState"/> instance of that type.
	/// See <see cref="Resolve"/> for more details about how <see cref="IState"/> instances are resolved by their types.
	/// This also allows to cache state instances so that they can be reused, instead of creating a new instance each time a state becomes the current state.
	/// See <see cref="IStateCache"/> for more details about state caches.
	/// </para>
	/// <para>
	/// SIGNALS: A signal is used to inform the current state about an event to which the current state might react (or not, depending on the current state and the signal).
	/// Basically, a signal is just an object of any type which is passed to the current state.
	/// A signal is issued using <see cref="Signal"/> or <see cref="Signal{T}"/>.
	/// The current state receives the signal using <see cref="IState.Signal"/>.
	/// Depending on the signal, the state might initiate a transition or perform some other state-specific actions.
	/// Signals are used to decouple the states from event sources so that events can be signalled independently of the current state.
	/// </para>
	/// <para>
	/// A very powerful concept is the combination of state inheritance and signals.
	/// Signals which are handled identically for all or a group of states can be handled in a base class which implements <see cref="IState"/>.
	/// Other states can then inherit from that base class and only add signal handling for their special state-specific cases.
	/// </para>
	/// <para>
	/// TRANSITIONS: Transitions are nothing else than changing the current state.
	/// A transition is initiated using <see cref="Transient"/> or <see cref="Transient{TState}"/>.
	/// During a transition, the previous states <see cref="IState.Leave"/> method is called, then <see cref="State"/> is updated to the next state, and then the next states <see cref="IState.Enter"/> method is called.
	/// </para>
	/// <para>
	/// DISPATCHING: Dispatching is a very important concept for creating state machines that behave in a logical way.
	/// When a signal or a transition is initiated (see above), the state machine does not immediately execute the signal or transition.
	/// Instead, it is handed to the dispatcher (<see cref="IStateDispatcher"/>), specified by the state machine configuration (<see cref="Configuration"/>, <see cref="StateMachineConfiguration.Dispatcher"/>).
	/// The dispatcher is the responsible for the actual execution of the signals and transitions.
	/// Usually, the signals and transitions are placed in a queue and are then processed one-by-one.
	/// However, this depends on the actual implementation of the <see cref="IStateDispatcher"/>.
	/// </para>
	/// <para>
	/// The reason why dispatching is important for a logical behaviour is the requirement that all state machine operations (signals and transitions) are completed before another operation is executed.
	/// This guarantees, for example, that all state code executed in <see cref="IState.Signal"/>, <see cref="IState.Enter"/> or <see cref="IState.Leave"/> is executed under the same current state, even if the state issues a signal or a transient.
	/// </para>
	/// </remarks>
	public class StateMachine
	{
		/// <summary>
		/// Creates a new instance of <see cref="StateMachine"/>.
		/// </summary>
		/// <param name="configuration">The state machines configuration.</param>
		/// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
		public StateMachine (StateMachineConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			this.Configuration = configuration;

			this.TransientDelegate = this.ExecuteTransient;
			this.SignalDelegate = this.ExecuteSignal;

			this.State = null;
		}




		/// <summary>
		/// Gets the state machine configuration.
		/// </summary>
		/// <value>
		/// The state machine configuration.
		/// </value>
		/// <remarks>
		/// <note type="note">
		/// Be careful when changing properties of <see cref="Configuration"/> while <see cref="State"/> is not null.
		/// See <see cref="StateMachineConfiguration"/> for more details.
		/// </note>
		/// </remarks>
		public StateMachineConfiguration Configuration { get; private set; }

		/// <summary>
		/// Gets the current state.
		/// </summary>
		/// <value>
		/// The current state or null if there is currently no state.
		/// </value>
		public IState State { get; private set; }




		/// <summary>
		/// Called when a transition is to be dispatched.
		/// </summary>
		/// <param name="transientInfo">The transition to dispatch.</param>
		/// <remarks>
		/// <para>
		/// The default implementation calls <see cref="IStateDispatcher.DispatchTransition"/> if the used <see cref="StateMachineConfiguration"/> (<see cref="Configuration"/>) provides an <see cref="IStateDispatcher"/> (<see cref="StateMachineConfiguration.Dispatcher"/>) or otherwise executes the transition immediately.
		/// </para>
		/// </remarks>
		protected virtual void DispatchTransient(StateTransientInfo transientInfo)
		{
			if (this.Configuration.Dispatcher == null)
			{
				this.TransientDelegate(transientInfo);
			}
			else
			{
				this.Configuration.Dispatcher.DispatchTransition(this.TransientDelegate, transientInfo);
			}
		}

		/// <summary>
		/// Called when a signal is to be dispatched.
		/// </summary>
		/// <param name="signalInfo">The signal to dispatch.</param>
		/// <remarks>
		/// <para>
		/// The default implementation calls <see cref="IStateDispatcher.DispatchSignal"/> if the used <see cref="StateMachineConfiguration"/> (<see cref="Configuration"/>) provides an <see cref="IStateDispatcher"/> (<see cref="StateMachineConfiguration.Dispatcher"/>) or otherwise executes the signal immediately.
		/// </para>
		/// </remarks>
		protected virtual void DispatchSignal(StateSignalInfo signalInfo)
		{
			if (this.Configuration.Dispatcher == null)
			{
				this.SignalDelegate(signalInfo);
			}
			else
			{
				this.Configuration.Dispatcher.DispatchSignal(this.SignalDelegate, signalInfo);
			}
		}

		/// <summary>
		/// Called when the state instance (<see cref="IState"/>) of a state type needs to be resolved.
		/// </summary>
		/// <param name="type">The type of the state to resolve.</param>
		/// <param name="useCache">Specifies whether the state can be retrieved from the state cache (if available).</param>
		/// <returns>
		/// The state instance or null if <paramref name="type"/> is null.
		/// </returns>
		/// <remarks>
		/// <para>
		/// If <paramref name="useCache"/> is true, a cache is available, and the cache already contains an instance of the state type, the state instance is retrieved from the cache.
		/// Otherwise, if the used <see cref="StateMachineConfiguration"/> (<see cref="Configuration"/>) provides an <see cref="IStateResolver"/> (<see cref="StateMachineConfiguration.Resolver"/>), that resolver is used.
		/// If no resolver is provided, it is attempted to directly create a new instance of the type specified by <paramref name="type"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="StateNotFoundException">The state instance specified by <paramref name="type"/> cannot be resolved.</exception>
		protected virtual IState Resolve (Type type, bool useCache)
		{
			if (type == null)
			{
				return null;
			}

			IState state;

			if (useCache && (this.Configuration.Cache != null) && this.Configuration.Cache.ContainsState(type))
			{
				state = this.Configuration.Cache.GetState(type);
			}
			else
			{
				if (this.Configuration.Resolver == null)
				{
					state = Activator.CreateInstance(type) as IState;
					if (state == null)
					{
						throw new StateNotFoundException(type);
					}
				}
				else
				{
					state = this.Configuration.Resolver.ResolveState(type);
					if (state == null)
					{
						throw new StateNotFoundException(type);
					}
				}
			}

			return state;
		}



		/// <summary>
		/// Initiates a transition to another state.
		/// </summary>
		/// <typeparam name="TState">The type of state to transition to.</typeparam>
		public void Transient<TState> ()
			where TState : IState
		{
			this.Transient(typeof(TState));
		}

		/// <summary>
		/// Initiates a transition to another state.
		/// </summary>
		/// <param name="state">The type of state to transition to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="state"/> is null.</exception>
		public void Transient (Type state)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			IState previousState = this.State;
			IState nextState = this.Resolve(state, true);

			StateTransientInfo transientInfo = new StateTransientInfo(this);
			transientInfo.NextState = nextState;
			transientInfo.PreviousState = previousState;

			this.DispatchTransient(transientInfo);
		}

		private StateMachineTransientDelegate TransientDelegate { get; set; }

		/// <summary>
		/// Called when a transition is eventually executed by the dispatcher (or by <see cref="DispatchTransient"/>) if no dispatcher is available).
		/// </summary>
		/// <param name="transientInfo">The transition to execute.</param>
		protected virtual void ExecuteTransient (StateTransientInfo transientInfo)
		{
			IState previousState = transientInfo.PreviousState;
			IState nextState = transientInfo.NextState;

			if (!object.ReferenceEquals(this.State, previousState))
			{
				return;
			}

			if (!((nextState?.IsInitialized).GetValueOrDefault(true)))
			{
				nextState?.Initialize(this);
			}

			this.OnBeforeLeave(transientInfo);
			this.State?.Leave(transientInfo);
			this.OnAfterLeave(transientInfo);

			this.State = nextState;

			this.OnBeforeEnter(transientInfo);
			this.State?.Enter(transientInfo);
			this.OnAfterEnter(transientInfo);

			if ((nextState != null) && (this.Configuration.Cache != null))
			{
				if (nextState.UseCaching && this.Configuration.EnableAutomaticCaching)
				{
					this.Configuration.Cache.AddState(nextState);
				}
			}
		}

		/// <summary>
		/// Sends a signal to the current state.
		/// </summary>
		/// <typeparam name="TSignal">The type of the signal.</typeparam>
		/// <param name="signal">The signal.</param>
		public void Signal<TSignal> (TSignal signal)
		{
			this.Signal((object)signal);
		}

		/// <summary>
		/// Sends a signal to the current state.
		/// </summary>
		/// <param name="signal">The signal.</param>
		public void Signal (object signal)
		{
			StateSignalInfo signalInfo = new StateSignalInfo(this);
			signalInfo.Signal = signal;

			this.DispatchSignal(signalInfo);
		}

		private StateMachineSignalDelegate SignalDelegate { get; set; }

		/// <summary>
		/// Called when a signal is eventually executed by the dispatcher (or by <see cref="DispatchSignal"/>) if no dispatcher is available).
		/// </summary>
		/// <param name="signalInfo">The signal to execute.</param>
		protected virtual void ExecuteSignal (StateSignalInfo signalInfo)
		{
			this.OnBeforeSignal(signalInfo);
			this.State?.Signal(signalInfo);
			this.OnAfterSignal(signalInfo);
		}



		/// <summary>
		/// Raised before the previous state is left during a transition.
		/// </summary>
		public event EventHandler<StateMachineTransientEventArgs> BeforeLeave;

		/// <summary>
		/// Raises <see cref="BeforeLeave"/>.
		/// </summary>
		/// <param name="transientInfo">The transition currently being executed.</param>
		protected virtual void OnBeforeLeave (StateTransientInfo transientInfo)
		{
			this.BeforeLeave?.Invoke(this, new StateMachineTransientEventArgs(transientInfo));
		}

		/// <summary>
		/// Raised after the previous state was left during a transition.
		/// </summary>
		public event EventHandler<StateMachineTransientEventArgs> AfterLeave;

		/// <summary>
		/// Raises <see cref="AfterLeave"/>.
		/// </summary>
		/// <param name="transientInfo">The transition currently being executed.</param>
		protected virtual void OnAfterLeave(StateTransientInfo transientInfo)
		{
			this.AfterLeave?.Invoke(this, new StateMachineTransientEventArgs(transientInfo));
		}

		/// <summary>
		/// Raised before the next state is entered during a transition.
		/// </summary>
		public event EventHandler<StateMachineTransientEventArgs> BeforeEnter;

		/// <summary>
		/// Raises <see cref="BeforeEnter"/>.
		/// </summary>
		/// <param name="transientInfo">The transition currently being executed.</param>
		protected virtual void OnBeforeEnter(StateTransientInfo transientInfo)
		{
			this.BeforeEnter?.Invoke(this, new StateMachineTransientEventArgs(transientInfo));
		}

		/// <summary>
		/// Raised after the next state was entered during a transition.
		/// </summary>
		public event EventHandler<StateMachineTransientEventArgs> AfterEnter;

		/// <summary>
		/// Raises <see cref="AfterEnter"/>.
		/// </summary>
		/// <param name="transientInfo">The transition currently being executed.</param>
		protected virtual void OnAfterEnter(StateTransientInfo transientInfo)
		{
			this.AfterEnter?.Invoke(this, new StateMachineTransientEventArgs(transientInfo));
		}

		/// <summary>
		/// Raised before a signal is passed to the current state.
		/// </summary>
		public event EventHandler<StateMachineSignalEventArgs> BeforeSignal;

		/// <summary>
		/// Raises <see cref="BeforeSignal"/>.
		/// </summary>
		/// <param name="signalInfo">The signal currently being executed.</param>
		protected virtual void OnBeforeSignal(StateSignalInfo signalInfo)
		{
			this.BeforeSignal?.Invoke(this, new StateMachineSignalEventArgs(signalInfo));
		}

		/// <summary>
		/// Raised after a signal was passed to the current state.
		/// </summary>
		public event EventHandler<StateMachineSignalEventArgs> AfterSignal;

		/// <summary>
		/// Raises <see cref="AfterSignal"/>.
		/// </summary>
		/// <param name="signalInfo">The signal currently being executed.</param>
		protected virtual void OnAfterSignal(StateSignalInfo signalInfo)
		{
			this.AfterSignal?.Invoke(this, new StateMachineSignalEventArgs(signalInfo));
		}
	}
}