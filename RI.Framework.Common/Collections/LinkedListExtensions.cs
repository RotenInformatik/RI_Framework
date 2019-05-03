﻿using System;
using System.Collections.Generic;




namespace RI.Framework.Collections
{
    /// <summary>
    ///     Provides utility/extension methods for the <see cref="LinkedList{T}" /> type.
    /// </summary>
    /// <threadsafety static="false" instance="false" />
    /// TODO: ToNodesForward
    /// TODO: ToNodesBackward
    public static class LinkedListExtensions
    {
        #region Static Methods

        /// <summary>
        ///     Enumerates the items of a linked list backwards.
        /// </summary>
        /// <typeparam name="T"> The type of the items in <paramref name="list" />. </typeparam>
        /// <param name="list"> The linked list. </param>
        /// <returns>
        ///     The <see cref="IEnumerable{T}" /> which enumerates the linked list items.
        /// </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="list" /> is null. </exception>
        public static IEnumerable<T> AsItemsBackward <T> (this LinkedList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            LinkedListNode<T> node = list.Last;
            while (node != null)
            {
                yield return node.Value;
                node = node.Previous;
            }
        }

        /// <summary>
        ///     Enumerates the items of a linked list forwards.
        /// </summary>
        /// <typeparam name="T"> The type of the items in <paramref name="list" />. </typeparam>
        /// <param name="list"> The linked list. </param>
        /// <returns>
        ///     The <see cref="IEnumerable{T}" /> which enumerates the linked list nodes.
        /// </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="list" /> is null. </exception>
        public static IEnumerable<T> AsItemsForward <T> (this LinkedList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            LinkedListNode<T> node = list.First;
            while (node != null)
            {
                yield return node.Value;
                node = node.Next;
            }
        }

        /// <summary>
        ///     Allows a linked list to be enumerated as its nodes, starting at the last node, rather than its values.
        /// </summary>
        /// <typeparam name="T"> The type of the items in <paramref name="list" />. </typeparam>
        /// <param name="list"> The linked list. </param>
        /// <returns>
        ///     The <see cref="IEnumerable{T}" /> which enumerates the linked list nodes.
        /// </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="list" /> is null. </exception>
        public static IEnumerable<LinkedListNode<T>> AsNodesBackward <T> (this LinkedList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            LinkedListNode<T> node = list.Last;
            while (node != null)
            {
                yield return node;
                node = node.Previous;
            }
        }

        /// <summary>
        ///     Allows a linked list to be enumerated as its nodes, starting at the first node, rather than its values.
        /// </summary>
        /// <typeparam name="T"> The type of the items in <paramref name="list" />. </typeparam>
        /// <param name="list"> The linked list. </param>
        /// <returns>
        ///     The <see cref="IEnumerable{T}" /> which enumerates the linked list nodes.
        /// </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="list" /> is null. </exception>
        public static IEnumerable<LinkedListNode<T>> AsNodesForward <T> (this LinkedList<T> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            LinkedListNode<T> node = list.First;
            while (node != null)
            {
                yield return node;
                node = node.Next;
            }
        }

        #endregion
    }
}
