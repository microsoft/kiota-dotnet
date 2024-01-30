// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif


namespace Microsoft.Kiota.Abstractions.Serialization;

/// <summary>
/// Set of helper methods for serialization
/// </summary>
public static partial class KiotaSerializer
{
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(string contentType, T value) where T : IParsable
    {
        using var writer = GetSerializationWriter(contentType, value);
        writer.WriteObjectValue(string.Empty, value);
        return writer.GetSerializedContent();
    }
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static string SerializeAsString<T>(string contentType, T value) where T : IParsable
    {
        using var stream = SerializeAsStream(contentType, value);
        return GetStringFromStream(stream);
    }
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(string contentType, IEnumerable<T> value) where T : IParsable
    {
        using var writer = GetSerializationWriter(contentType, value);
        writer.WriteCollectionOfObjectValues(string.Empty, value);
        return writer.GetSerializedContent();
    }
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static string SerializeAsString<T>(string contentType, IEnumerable<T> value) where T : IParsable
    {
        using var stream = SerializeAsStream(contentType, value);
        return GetStringFromStream(stream);
    }
    private static string GetStringFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    private static ISerializationWriter GetSerializationWriter(string contentType, object value)
    {
        if(string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
        if(value == null) throw new ArgumentNullException(nameof(value));
        return SerializationWriterFactoryRegistry.DefaultInstance.GetSerializationWriter(contentType);
    }
}