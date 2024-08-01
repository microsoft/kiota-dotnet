// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Abstractions.Serialization;

public static partial class KiotaJsonSerializer
{
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default a backing store is used, and you'll only get changed properties</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(T value, bool serializeOnlyChangedValues = true) where T : IParsable
    => KiotaSerializer.SerializeAsStream(_jsonContentType, value, serializeOnlyChangedValues);

    /// <summary>
    /// Serializes the specified object as a string based JSON stream.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default a backing store is used, and you'll only get changed properties</param>
    /// <returns>A <see cref="Stream"/> containing the serialized JSON data.</returns>

    public static Stream SerializeAsJsonStream<T>(this T value, bool serializeOnlyChangedValues = true) where T : IParsable
    => SerializeAsStream(value, serializeOnlyChangedValues);

    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized representation as a string.</returns>
    [Obsolete("This method is obsolete, use the async method instead")]
    public static string SerializeAsString<T>(T value) where T : IParsable
    => KiotaSerializer.SerializeAsString(_jsonContentType, value);

    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Task<string> SerializeAsStringAsync<T>(T value, CancellationToken cancellationToken) where T : IParsable => SerializeAsStringAsync(value, true, cancellationToken);

    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default a backing store is used, and you'll only get changed properties</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static Task<string> SerializeAsStringAsync<T>(T value, bool serializeOnlyChangedValues = true, CancellationToken cancellationToken = default) where T : IParsable
    => KiotaSerializer.SerializeAsStringAsync(_jsonContentType, value, serializeOnlyChangedValues, cancellationToken);

    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default a backing store is used, and you'll only get changed properties</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsStream<T>(IEnumerable<T> value, bool serializeOnlyChangedValues = true) where T : IParsable
    => KiotaSerializer.SerializeAsStream(_jsonContentType, value, serializeOnlyChangedValues);

    /// <summary>
    /// Serializes the specified object as a string based JSON stream.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The enumerable of objects to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default a backing store is used, and you'll only get changed properties</param>
    /// <returns>A <see cref="Stream"/> containing the serialized JSON data.</returns>

    public static Stream SerializeAsJsonStream<T>(this IEnumerable<T> value, bool serializeOnlyChangedValues = true) where T : IParsable
    => SerializeAsStream(value, serializeOnlyChangedValues);

    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>The serialized representation as a string.</returns>
    [Obsolete("This method is obsolete, use the async method instead")]
    public static string SerializeAsString<T>(IEnumerable<T> value) where T : IParsable
    => KiotaSerializer.SerializeAsString(_jsonContentType, value);
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Task<string> SerializeAsStringAsync<T>(IEnumerable<T> value, CancellationToken cancellationToken) where T : IParsable => SerializeAsStringAsync(value, true, cancellationToken);
    /// <summary>
    /// Serializes the given object into a string based on the content type.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">By default a backing store is used, and you'll only get changed properties</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static Task<string> SerializeAsStringAsync<T>(IEnumerable<T> value, bool serializeOnlyChangedValues = true, CancellationToken cancellationToken = default) where T : IParsable
    => KiotaSerializer.SerializeAsStringAsync(_jsonContentType, value, serializeOnlyChangedValues, cancellationToken);

}