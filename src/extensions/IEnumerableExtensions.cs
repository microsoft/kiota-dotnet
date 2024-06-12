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
        public static List<T> AsList<T>(this IEnumerable<T> e)
        {
            if (e is null) ThrowHelper.ThrowArgumentNullException(nameof(e));
            
            if (e is List<T> list) return list;

            return new List<T>(e);
        }

        public static T[] AsArray<T>(this IEnumerable<T> e)
        {
            if (e is null) ThrowHelper.ThrowArgumentNullException(nameof(e));

            if (e is T[] array) return array;

            if (e is ICollection<T> collection)
            {
                // Allocate an array with the exact size
#if NET5_0_OR_GREATER
                T[] result = GC.AllocateUninitializedArray<T>(collection.Count);
#else
                T[] result = new T[count];
#endif
                collection.CopyTo(result, 0);
                return result;
            }

            // First pass to count the elements
            int count = 0;
            foreach (var item in e) count++;

            // Allocate array with the counted size
#if NET5_0_OR_GREATER
            T[] result = GC.AllocateUninitializedArray<T>(count);
#else
            T[] result = new T[count];
#endif

            // Second pass to copy the elements
            count = 0;
            foreach (var item in e) result[count++] = item;
            return default(TValue);
        }
    }
}
