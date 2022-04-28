// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Kiota.Abstractions.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="IDictionary"/>
    /// </summary>
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// Try to add the element to the <see cref="IDictionary"/> instance.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="dictionary">The dictionary to add to.</param>
        /// <param name="key">The key parameter.</param>
        /// <param name="value">The value</param>
        /// <returns>True or False based on whether the item is added successfully</returns>
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if(dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            
            if(key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if(!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Adds or replaces the element to the <see cref="IDictionary"/> instance.
        /// </summary>
        /// <typeparam name="TKey"> The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="dictionary">The dictionary to add to.</param>
        /// <param name="key">The key parameter.</param>
        /// <param name="value">The value</param>
        /// <returns>The previous value if any</returns>
        public static TValue AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if(!dictionary.TryAdd(key, value))
            {
                var oldValue = dictionary[key];
                dictionary[key] = value;
                return oldValue;
            }
            return default(TValue);
        }

    }
}
