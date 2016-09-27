﻿using System;
using System.Collections.Generic;

using RI.Framework.Collections;
using RI.Framework.Composition;
using RI.Framework.Composition.Model;
using RI.Framework.Services.Logging;
using RI.Framework.Utilities;
using RI.Framework.Utilities.Exceptions;




namespace RI.Framework.Services.Regions
{
	/// <summary>
	///     Implements a default region service which is suitable for most scenarios.
	/// </summary>
	/// <remarks>
	///     <para>
	///         This region service manages <see cref="IRegionAdapter" />s from two sources.
	///         One are the explicitly specified adapters added through <see cref="AddAdapter" />.
	///         The second is a <see cref="CompositionContainer" /> if this <see cref="RegionService" /> is added as an export (the adapters are then imported through composition).
	///         <see cref="Adapters" /> gives the sequence containing all adapters from all sources.
	///     </para>
	/// </remarks>
	public sealed class RegionService : IRegionService
	{
		#region Instance Constructor/Destructor

		/// <summary>
		///     Creates a new instance of <see cref="RegionService" />.
		/// </summary>
		public RegionService ()
		{
			this.AdaptersManual = new List<IRegionAdapter>();
			this.RegionDictionary = new Dictionary<string, Tuple<object, IRegionAdapter>>(StringComparer.InvariantCultureIgnoreCase);
		}

		#endregion




		#region Instance Properties/Indexer

		[ImportProperty (typeof(IRegionAdapter), Recomposable = true)]
		private Import AdaptersImported { get; set; }

		private List<IRegionAdapter> AdaptersManual { get; set; }

		private Dictionary<string, Tuple<object, IRegionAdapter>> RegionDictionary { get; set; }

		#endregion




		#region Instance Methods

		private void Log (string format, params object[] args)
		{
			LogLocator.LogDebug(this.GetType().Name, format, args);
		}

		#endregion




		#region Interface: IRegionService

		/// <inheritdoc />
		public IEnumerable<IRegionAdapter> Adapters
		{
			get
			{
				foreach (IRegionAdapter adapter in this.AdaptersManual)
				{
					yield return adapter;
				}

				foreach (IRegionAdapter adapter in this.AdaptersImported.Values<IRegionAdapter>())
				{
					yield return adapter;
				}
			}
		}

		/// <inheritdoc />
		public IEnumerable<string> Regions
		{
			get
			{
				return this.RegionDictionary.Keys;
			}
		}

		/// <inheritdoc />
		public void ActivateElement (string region, object element)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				throw new InvalidOperationException();
			}

			if (!this.HasElement(region, element))
			{
				this.AddElement(region, element);
			}

			this.DeactivateAllElements(region);

			object container = this.RegionDictionary[region].Item1;
			IRegionAdapter adapter = this.RegionDictionary[region].Item2;

			adapter.Activate(container, element);
		}

		/// <inheritdoc />
		public void AddAdapter (IRegionAdapter regionAdapter)
		{
			if (regionAdapter == null)
			{
				throw new ArgumentNullException(nameof(regionAdapter));
			}

			if (this.AdaptersManual.Contains(regionAdapter))
			{
				return;
			}

			this.Log("Region adapter added: {0}", regionAdapter.GetType().Name);

			this.AdaptersManual.Add(regionAdapter);
		}

		/// <inheritdoc />
		public void AddElement (string region, object element)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				throw new InvalidOperationException();
			}

			if (this.HasElement(region, element))
			{
				return;
			}

			object container = this.RegionDictionary[region].Item1;
			IRegionAdapter adapter = this.RegionDictionary[region].Item2;

			adapter.Add(container, element);
		}

		/// <inheritdoc />
		public void AddRegion (string region, object container)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			List<Tuple<int, IRegionAdapter>> adapters = new List<Tuple<int, IRegionAdapter>>();
			Type containerType = container.GetType();
			foreach (IRegionAdapter currentAdapter in this.Adapters)
			{
				int inheritanceDepth = 0;
				if (currentAdapter.IsCompatibleContainer(containerType, out inheritanceDepth))
				{
					adapters.Add(new Tuple<int, IRegionAdapter>(inheritanceDepth, currentAdapter));
				}
			}
			adapters.Sort((x, y) => x.Item1.CompareTo(y.Item1));
			if (adapters.Count == 0)
			{
				throw new InvalidTypeArgumentException(nameof(container));
			}
			IRegionAdapter adapter = adapters[0].Item2;

			if (this.RegionDictionary.ContainsKey(region))
			{
				if (object.ReferenceEquals(container, this.RegionDictionary[region].Item1) && adapter.Equals(this.RegionDictionary[region].Item2))
				{
					return;
				}

				this.ClearElements(region);
				this.RegionDictionary.Remove(region);
			}

			this.Log("Region added: {0} -> {1} @ {2}", region, container.GetType().Name, adapter.GetType().Name);

			this.RegionDictionary.Add(region, new Tuple<object, IRegionAdapter>(container, adapter));
		}

		/// <inheritdoc />
		public void ClearElements (string region)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				throw new InvalidOperationException();
			}

			this.DeactivateAllElements(region);

			object container = this.RegionDictionary[region].Item1;
			IRegionAdapter adapter = this.RegionDictionary[region].Item2;

			adapter.Clear(container);
		}

		/// <inheritdoc />
		public void DeactivateAllElements (string region)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				throw new InvalidOperationException();
			}

			object container = this.RegionDictionary[region].Item1;
			IRegionAdapter adapter = this.RegionDictionary[region].Item2;
			List<object> elements = adapter.Get(container);

			foreach (object element in elements)
			{
				adapter.Deactivate(container, element);
			}
		}

		/// <inheritdoc />
		public List<object> GetElements (string region)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				return null;
			}

			object container = this.RegionDictionary[region].Item1;
			IRegionAdapter adapter = this.RegionDictionary[region].Item2;

			return adapter.Get(container);
		}

		/// <inheritdoc />
		public object GetRegionContainer (string region)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				return null;
			}

			return this.RegionDictionary[region].Item1;
		}

		/// <inheritdoc />
		public string GetRegionName (object container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			foreach (KeyValuePair<string, Tuple<object, IRegionAdapter>> region in this.RegionDictionary)
			{
				if (object.ReferenceEquals(region.Value.Item1, container))
				{
					return region.Key;
				}
			}

			return null;
		}

		/// <inheritdoc />
		public HashSet<string> GetRegionNames (object container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			HashSet<string> names = new HashSet<string>(this.RegionDictionary.Comparer);
			foreach (KeyValuePair<string, Tuple<object, IRegionAdapter>> region in this.RegionDictionary)
			{
				if (object.ReferenceEquals(region.Value.Item1, container))
				{
					names.Add(region.Key);
				}
			}
			return names;
		}

		/// <inheritdoc />
		public bool HasElement (string region, object element)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				return false;
			}

			List<object> elements = this.GetElements(region);
			return elements.Any(x => object.ReferenceEquals(x, element));
		}

		/// <inheritdoc />
		public bool HasRegion (string region)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			return this.RegionDictionary.ContainsKey(region);
		}

		/// <inheritdoc />
		public void RemoveAdapter (IRegionAdapter regionAdapter)
		{
			if (regionAdapter == null)
			{
				throw new ArgumentNullException(nameof(regionAdapter));
			}

			foreach (KeyValuePair<string, Tuple<object, IRegionAdapter>> region in this.RegionDictionary)
			{
				if (regionAdapter.Equals(region.Value.Item2))
				{
					throw new InvalidOperationException();
				}
			}

			if (!this.AdaptersManual.Contains(regionAdapter))
			{
				return;
			}

			this.Log("Region adapter removed: {0}", regionAdapter.GetType().Name);

			this.AdaptersManual.RemoveAll(regionAdapter);
		}

		/// <inheritdoc />
		public void RemoveElement (string region, object element)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				throw new InvalidOperationException();
			}

			if (!this.HasElement(region, element))
			{
				return;
			}

			object container = this.RegionDictionary[region].Item1;
			IRegionAdapter adapter = this.RegionDictionary[region].Item2;

			adapter.Deactivate(container, element);
			adapter.Remove(container, element);
		}

		/// <inheritdoc />
		public void RemoveRegion (string region)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				return;
			}

			this.ClearElements(region);

			this.Log("Region removed: {0}", region);

			this.RegionDictionary.Remove(region);
		}

		/// <inheritdoc />
		public void SetElement (string region, object element)
		{
			if (region == null)
			{
				throw new ArgumentNullException(nameof(region));
			}

			if (region.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(region));
			}

			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!this.RegionDictionary.ContainsKey(region))
			{
				throw new InvalidOperationException();
			}

			this.ClearElements(region);
			this.AddElement(region, element);
		}

		#endregion
	}
}