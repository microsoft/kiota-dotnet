// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  
//  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Kiota.Abstractions.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="e">The enumerable to convert.</param>
        /// <returns>A <see cref="List{T}"/> containing the elements of the enumerable, or <c>null</c> if the input is <c>null</c>.</returns>
        public static List<T>? AsList<T>(this IEnumerable<T>? e)
        {
            if (e is null) return null;

            if (e is List<T> list) return list;

            return new List<T>(e);
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to an array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="e">The enumerable to convert.</param>
        /// <returns>An array containing the elements of the enumerable, or <c>null</c> if the input is <c>null</c>.</returns>
        public static T[]? AsArray<T>(this IEnumerable<T>? e)
        {
            if (e is null) return null;

            if (e is T[] array) return array;

            T[]? result = null;

            if (e is ICollection<T> collection)
            {
                // Allocate an array with the exact size
                result = AllocateOnHeap(collection.Count);
                collection.CopyTo(result, 0);
                return result;
            }

            // First pass to count the elements
            int count = 0;
            foreach (var item in e) count++;

            result = AllocateOnHeap(count);

            // Second pass to copy the elements
            count = 0;
            foreach (var item in e) result[count++] = item;
            return result;

            static T[] AllocateOnHeap(int count)
            {
#if NET5_0_OR_GREATER
                return GC.AllocateUninitializedArray<T>(count);
#else
                return new T[count];
#endif
            }
        }
    }
}
