﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;




namespace RI.Framework.Collections.ObjectModel
{
	/// <summary>
	///     Provides utility/extension methods for the <see cref="IPool{T}" /> type and its implementations.
	/// </summary>
	[SuppressMessage ("ReSharper", "InconsistentNaming")]
	public static class IPoolExtensions
	{
		#region Static Methods

		/// <summary>
		///     Changes a pool so that it contains a specified exact number of free items.
		/// </summary>
		/// <typeparam name="T"> The type of the items in <paramref name="pool" />. </typeparam>
		/// <param name="pool"> The pool. </param>
		/// <param name="numItems"> The number of free items the pool must have. </param>
		/// <returns>
		///     The change in free items which was necessary to get the specified number of free items.
		///     The value is positive if new items were created, negative if free items were removed, or zero if the pool already contained the specified number of free items.
		/// </returns>
		/// <exception cref="ArgumentNullException"> <paramref name="pool" /> is null. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> <paramref name="numItems" /> is less than zero. </exception>
		public static int ReduceEnsure <T> (this IPool<T> pool, int numItems)
		{
			if (pool == null)
			{
				throw new ArgumentNullException(nameof(pool));
			}

			if (numItems < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(numItems));
			}

			int difference = 0;
			difference -= pool.Reduce(numItems);
			difference += pool.Ensure(numItems);
			return difference;
		}

		/// <summary>
		///     Returns multiple items to a pool for recycling.
		/// </summary>
		/// <typeparam name="T"> The type of the items in <paramref name="pool" />. </typeparam>
		/// <param name="pool"> The pool. </param>
		/// <param name="items"> The sequence of items to be returned to the pool. </param>
		/// <returns>
		///     The number of items returned to the pool.
		///     Zero if the sequence contained no elements.
		/// </returns>
		/// <remarks>
		///     <para>
		///         <paramref name="items" /> is enumerated exactly once.
		///     </para>
		///     <note type="note">
		///         The behaviour when the same item is returned multiple times without being taken is defined by the <see cref="IPool{T}" /> implementation.
		///     </note>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="pool" /> or <paramref name="items" /> is null. </exception>
		public static int ReturnRange <T> (this IPool<T> pool, IEnumerable<T> items)
		{
			if (pool == null)
			{
				throw new ArgumentNullException(nameof(pool));
			}

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			int count = 0;
			foreach (T item in items)
			{
				pool.Return(item);
				count++;
			}
			return count;
		}

		/// <summary>
		///     Returns multiple items to a pool for recycling.
		/// </summary>
		/// <typeparam name="T"> The type of the items in <paramref name="pool" />. </typeparam>
		/// <param name="pool"> The pool. </param>
		/// <param name="items"> The list of items to be returned to the pool. </param>
		/// <returns>
		///     The number of items returned to the pool.
		///     Zero if the list contained no elements.
		/// </returns>
		/// <remarks>
		///     <para>
		///         <paramref name="items" /> is enumerated exactly once.
		///     </para>
		///     <note type="note">
		///         The behaviour when the same item is returned multiple times without being taken is defined by the <see cref="IPool{T}" /> implementation.
		///     </note>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="pool" /> or <paramref name="items" /> is null. </exception>
		public static int ReturnRange <T> (this IPool<T> pool, IList<T> items)
		{
			if (pool == null)
			{
				throw new ArgumentNullException(nameof(pool));
			}

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			int count = 0;
			for (int i1 = 0; i1 < items.Count; i1++)
			{
				T item = items[i1];
				pool.Return(item);
				count++;
			}
			return count;
		}

		/// <summary>
		///     Returns multiple items to a pool for recycling.
		/// </summary>
		/// <typeparam name="T"> The type of the items in <paramref name="pool" />. </typeparam>
		/// <param name="pool"> The pool. </param>
		/// <param name="items"> The array of items to be returned to the pool. </param>
		/// <returns>
		///     The number of items returned to the pool.
		///     Zero if the array contained no elements.
		/// </returns>
		/// <remarks>
		///     <para>
		///         <paramref name="items" /> is enumerated exactly once.
		///     </para>
		///     <note type="note">
		///         The behaviour when the same item is returned multiple times without being taken is defined by the <see cref="IPool{T}" /> implementation.
		///     </note>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="pool" /> or <paramref name="items" /> is null. </exception>
		public static int ReturnRange <T> (this IPool<T> pool, T[] items)
		{
			if (pool == null)
			{
				throw new ArgumentNullException(nameof(pool));
			}

			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			int count = 0;
			for (int i1 = 0; i1 < items.Length; i1++)
			{
				T item = items[i1];
				pool.Return(item);
				count++;
			}
			return count;
		}

		/// <summary>
		///     Takes multiple items from a pool and creates as much new items as necessary.
		/// </summary>
		/// <typeparam name="T"> The type of the items in <paramref name="pool" />. </typeparam>
		/// <param name="pool"> The pool. </param>
		/// <param name="numItems"> The number of items to take. </param>
		/// <returns>
		///     The array of taken items.
		/// </returns>
		/// <exception cref="ArgumentNullException"> <paramref name="pool" /> is null. </exception>
		/// <exception cref="ArgumentOutOfRangeException"> <paramref name="numItems" /> is less than zero. </exception>
		public static T[] TakeRange <T> (this IPool<T> pool, int numItems)
		{
			if (pool == null)
			{
				throw new ArgumentNullException(nameof(pool));
			}

			if (numItems < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(numItems));
			}

			T[] items = new T[numItems];
			for (int i1 = 0; i1 < numItems; i1++)
			{
				items[i1] = pool.Take();
			}
			return items;
		}

		#endregion
	}
}
