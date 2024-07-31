// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Store;

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
    /// <param name="serializeOnlyChangedValues">By default you'll only get the changed properties.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(string contentType, T value, bool serializeOnlyChangedValues = true) where T : IParsable
    {
        bool restoreInitializationCompleted = false;
        if(!serializeOnlyChangedValues && value is IBackedModel backedModel)
        {
            // reset the initialization completed flag to ensure all properties are serialized
            restoreInitializationCompleted = backedModel.BackingStore.InitializationCompleted;
            backedModel.BackingStore.InitializationCompleted = false;
        }
        using var writer = GetSerializationWriter(contentType, value);
        writer.WriteObjectValue(null, value);
        var stream = writer.GetSerializedContent();
        if(restoreInitializationCompleted)
        {
            (value as IBackedModel)!.BackingStore.InitializationCompleted = true;
        }
        return stream;
    }
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized representation as a string.</returns>
    [Obsolete("This method is obsolete, use the async method instead")]
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
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static Task<string> SerializeAsStringAsync<T>(string contentType, T value, CancellationToken cancellationToken = default) where T : IParsable
    {
        using var stream = SerializeAsStream(contentType, value);
        return GetStringFromStreamAsync(stream, cancellationToken);
    }
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default you'll only get the changed properties.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(string contentType, IEnumerable<T> value, bool serializeOnlyChangedValues = true) where T : IParsable
    {
        bool resetInitializationCompleted = false;
        if(!serializeOnlyChangedValues && value is IBackedModel backedModel)
        {
            // reset the initialization completed flag to ensure all properties are serialized
            backedModel.BackingStore.InitializationCompleted = false;
            resetInitializationCompleted = true;
        }
        using var writer = GetSerializationWriter(contentType, value);
        writer.WriteCollectionOfObjectValues(null, value);
        var stream = writer.GetSerializedContent();
        if(resetInitializationCompleted)
        {
            (value as IBackedModel)!.BackingStore.InitializationCompleted = true;
        }
        return stream;
    }
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="contentType">Content type to serialize the object to </param>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized representation as a string.</returns>
    [Obsolete("This method is obsolete, use the async method instead")]
    public static string SerializeAsString<T>(string contentType, IEnumerable<T> value) where T : IParsable
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
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static Task<string> SerializeAsStringAsync<T>(string contentType, IEnumerable<T> value, CancellationToken cancellationToken = default) where T : IParsable
    {
        using var stream = SerializeAsStream(contentType, value);
        return GetStringFromStreamAsync(stream, cancellationToken);
    }
    [Obsolete("This method is obsolete, use the async method instead")]
    private static string GetStringFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    private static async Task<string> GetStringFromStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);
#if NET7_0_OR_GREATER
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#else
        return await reader.ReadToEndAsync().ConfigureAwait(false);
#endif
    }
    private static ISerializationWriter GetSerializationWriter(string contentType, object value)
    {
        if(string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
        if(value == null) throw new ArgumentNullException(nameof(value));
        return SerializationWriterFactoryRegistry.DefaultInstance.GetSerializationWriter(contentType);
    }
}