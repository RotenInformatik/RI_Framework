﻿using System;
using System.Collections.Generic;
using System.Linq;

using RI.Framework.Utilities;




namespace RI.Framework.Services.Resources
{
	/// <summary>
	///     Provides utility/extension methods for the <see cref="IResourceService" /> type.
	/// </summary>
	public static class IResourceServiceExtensions
	{
		#region Static Methods

		/// <summary>
		///     Gets the names of all available resources.
		/// </summary>
		/// <param name="resourceService"> The resource service. </param>
		/// <returns>
		///     The hash set with the names of all available resources.
		///     If no resources are available, an empty hash set is returned.
		/// </returns>
		public static HashSet<string> GetAvailableResourceNames (this IResourceService resourceService)
		{
			if (resourceService == null)
			{
				throw new ArgumentNullException(nameof(resourceService));
			}

			HashSet<string> resources = new HashSet<string>(StringComparerEx.InvariantCultureIgnoreCase);
			foreach (IResourceSet set in resourceService.LoadedSets)
			{
				foreach (string resource in set.AvailableResources)
				{
					resources.Add(resource);
				}
			}
			return resources;
		}

		/// <summary>
		///     Gets the IDs of all loaded resource sets.
		/// </summary>
		/// <param name="resourceService"> The resource service. </param>
		/// <returns>
		///     The hash set with the IDs of all loaded resource sets.
		///     If no resource sets are loaded, an empty hash set is returned.
		/// </returns>
		/// <exception cref="ArgumentNullException"> <paramref name="resourceService" /> is null. </exception>
		public static HashSet<string> GetLoadedSetIds (this IResourceService resourceService)
		{
			if (resourceService == null)
			{
				throw new ArgumentNullException(nameof(resourceService));
			}

			HashSet<string> ids = new HashSet<string>(from x in resourceService.LoadedSets select x.Id, StringComparerEx.InvariantCultureIgnoreCase);
			return ids;
		}

		/// <summary>
		///     Loads all the resource sets whose ID are in a specified sequence of resource set IDs.
		/// </summary>
		/// <param name="resourceService"> The resource service. </param>
		/// <param name="setIdsToLoad"> The resourceset IDs of the resource sets to load. </param>
		/// <param name="lazyLoad"> Specifies whether lazy loading shall be used for the resources of this resource set or not. </param>
		/// <remarks>
		///     <para>
		///         <see cref="SetLoadedSetIds" /> calls <see cref="IResourceSet.Load" /> for all found sets which match with an ID from <paramref name="setIdsToLoad" />.
		///         IDs in <paramref name="setIdsToLoad" /> which are not found in <see cref="IResourceService.AvailableSets" /> are ignored and will not be loaded.
		///     </para>
		///     <para>
		///         The resource sets are loaded in addition to any already loaded sets.
		///         No sets will be unloaded by <see cref="SetLoadedSetIds" />.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="resourceService" /> or <paramref name="setIdsToLoad" /> is null. </exception>
		public static void SetLoadedSetIds (this IResourceService resourceService, IEnumerable<string> setIdsToLoad, bool lazyLoad)
		{
			if (resourceService == null)
			{
				throw new ArgumentNullException(nameof(resourceService));
			}

			if (setIdsToLoad == null)
			{
				throw new ArgumentNullException(nameof(setIdsToLoad));
			}

			HashSet<string> idsToLoad = new HashSet<string>(setIdsToLoad, StringComparerEx.InvariantCultureIgnoreCase);
			IEnumerable<IResourceSet> setsToLoad = from x in resourceService.AvailableSets where idsToLoad.Contains(x.Id) select x;
			foreach (IResourceSet setToLoad in setsToLoad)
			{
				setToLoad.Load(lazyLoad);
			}
		}

		#endregion
	}
}
