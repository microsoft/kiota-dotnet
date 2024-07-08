// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Kiota.Abstractions.Serialization;

/// <summary>
/// Helper methods for intersection wrappers
/// </summary>
public static class ParseNodeHelper
{
    /// <summary>
    /// Merges the given fields deserializers for an intersection type into a single collection.
    /// </summary>
    /// <param name="targets">The collection of deserializers to merge.</param>
    public static IDictionary<string, Action<IParseNode>> MergeDeserializersForIntersectionWrapper(params IParsable?[] targets)
    {
        if(targets == null)
        {
            throw new ArgumentNullException(nameof(targets));
        }
        if(targets.Length == 0)
        {
            throw new ArgumentException("At least one target must be provided.", nameof(targets));
        }

        var result = new Dictionary<string, Action<IParseNode>>();
        foreach(var target in targets)
        {
            if(target != null)
            {
                var fieldDeserializers = target.GetFieldDeserializers();
                foreach(var fieldDeserializer in fieldDeserializers)
                {
                    if(!result.ContainsKey(fieldDeserializer.Key))
                    {
                        result.Add(fieldDeserializer.Key, fieldDeserializer.Value);
                    }
                }
            }
        }

        return result;
    }
}