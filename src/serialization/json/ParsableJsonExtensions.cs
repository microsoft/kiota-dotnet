using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization;

/// <summary>
/// Extension methods for <see cref="IParsable"/> instances specifically for JSON.
/// </summary>
public static class ParsableJsonExtensions
{
    private const string _contentType = "application/json";

    /// <summary>
    /// Serializes the given object into a json stream
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsJsonStream<T>(this T value, bool serializeOnlyChangedValues = false) where T : IParsable
    => value.SerializeAsStream(_contentType, serializeOnlyChangedValues);

    /// <summary>
    /// Serializes the given object into a json string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <param name="cancellationToken">Cancel the request during execution.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static async Task<string> SerializeAsJsonStringAsync<T>(this T value, bool serializeOnlyChangedValues = false, CancellationToken cancellationToken = default) where T : IParsable
    => await value.SerializeAsStringAsync(_contentType, serializeOnlyChangedValues, cancellationToken);

    /// <summary>
    /// Serializes the given collection into a json stream.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsJsonStream<T>(this IEnumerable<T> value, bool serializeOnlyChangedValues = false) where T : IParsable
    => value.SerializeAsStream(_contentType, serializeOnlyChangedValues);

    /// <summary>
    /// Serializes the given collection into a json string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <param name="cancellationToken">Cancel the request during execution.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static async Task<string> SerializeAsJsonStringAsync<T>(this IEnumerable<T> value, bool serializeOnlyChangedValues = false, CancellationToken cancellationToken = default) where T : IParsable
    => await value.SerializeAsStringAsync(_contentType, serializeOnlyChangedValues, cancellationToken);

    /// <summary>
    /// Serializes the given collection into a json stream.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <returns>The serialized representation as a stream.</returns>
    public static Stream SerializeAsJsonStream<T>(this T[] value, bool serializeOnlyChangedValues = false) where T : IParsable
    => value.SerializeAsStream(_contentType, serializeOnlyChangedValues);

    /// <summary>
    /// Serializes the given collection into a json string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="serializeOnlyChangedValues">If this object uses the <see cref="Abstractions.Store.IBackingStore"/>, use this to control if you want all properties or just the changed once.</param>
    /// <param name="cancellationToken">Cancel the request during execution.</param>
    /// <returns>The serialized representation as a string.</returns>
    public static async Task<string> SerializeAsJsonStringAsync<T>(this T[] value, bool serializeOnlyChangedValues = false, CancellationToken cancellationToken = default) where T : IParsable
    => await value.SerializeAsStringAsync(_contentType, serializeOnlyChangedValues, cancellationToken);
}