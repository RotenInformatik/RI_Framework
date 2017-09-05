﻿using System;
using System.Collections.Specialized;
using System.Windows;




namespace RI.Framework.Utilities.Wpf.Markup
{
	/// <summary>
	/// The collection which holds all event-to-command bindings (<see cref="EventBinding"/>) used with <see cref="EventToCommandBinder"/>.
	/// </summary>
	public sealed class EventBindings : FreezableCollection<EventBinding>
	{
		private DependencyObject AttachedTo { get; }

		internal EventBindings(DependencyObject attachedTo)
		{
			if (attachedTo == null)
			{
				throw new ArgumentNullException(nameof(attachedTo));
			}

			this.AttachedTo = attachedTo;

			((INotifyCollectionChanged)this).CollectionChanged += (sender, args) =>
			{
				if (args.OldItems != null)
				{
					foreach (object oldItem in args.OldItems)
					{
						(oldItem as EventBinding)?.Detach();
					}
				}

				if (args.NewItems != null)
				{
					foreach (object newItem in args.NewItems)
					{
						(newItem as EventBinding)?.Attach(this.AttachedTo);
					}
				}
			};
		}
	}
}