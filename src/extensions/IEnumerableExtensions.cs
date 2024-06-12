// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Kiota.Abstractions.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable"/>
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="e">The enumerable to convert.</param>
        /// <returns>A <see cref="List{T}"/> containing the elements of the enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input enumerable is null.</exception>
        public static List<T> AsList<T>(this IEnumerable<T> e)
        {
            if (e is null) throw new ArgumentNullException(nameof(e), "Input collection cannot be null.");
            
            if (e is List<T> list) return list;

            return new List<T>(e);
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to an array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="e">The enumerable to convert.</param>
        /// <returns>An array containing the elements of the enumerable.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input enumerable is null.</exception>
        public static T[] AsArray<T>(this IEnumerable<T> e)
        {
            if (e is null) throw new ArgumentNullException(nameof(e), "Input collection cannot be null.");

            if (e is T[] array) return array;

            T[]? result = null;

            if (e is ICollection<T> collection)
            {
                // Allocate an array with the exact size
#if NET5_0_OR_GREATER
                result = GC.AllocateUninitializedArray<T>(collection.Count);
#else
                result = new T[collection.Count];
#endif
                collection.CopyTo(result, 0);
                return result;
            }

            // First pass to count the elements
            int count = 0;
            foreach (var item in e) count++;

            // Allocate array with the counted size
#if NET5_0_OR_GREATER
            result = GC.AllocateUninitializedArray<T>(count);
#else
            result = new T[count];
#endif

            // Second pass to copy the elements
            count = 0;
            foreach (var item in e) result[count++] = item;
            return result;
        }
    }
}
