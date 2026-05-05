// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Abstractions.Serialization;

/// <summary>
/// Set of helper methods for JSON serialization
/// </summary>
public static partial class KiotaJsonSerializer
{
    private const string _jsonContentType = "application/json";

    /// <summary>
    /// Deserializes the given stream into an object.
    /// </summary>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<T?> DeserializeAsync<T>(string serializedRepresentation, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
    => KiotaSerializer.DeserializeAsync(_jsonContentType, serializedRepresentation, parsableFactory, cancellationToken);
    /// <summary>
    /// Deserializes the given stream into an object.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<T?> DeserializeAsync<T>(Stream stream, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
    => KiotaSerializer.DeserializeAsync(_jsonContentType, stream, parsableFactory, cancellationToken);
    /// <summary>
    /// Deserializes the given stream into an object.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET5_0_OR_GREATER
    public static Task<T?> DeserializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(Stream stream, CancellationToken cancellationToken = default) where T : IParsable
#else
    public static Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default) where T : IParsable
#endif
    => KiotaSerializer.DeserializeAsync<T>(_jsonContentType, stream, cancellationToken);
    /// <summary>
    /// Deserializes the given stream into an object.
    /// </summary>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET5_0_OR_GREATER
    public static Task<T?> DeserializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
#else
    public static Task<T?> DeserializeAsync<T>(string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
#endif
    => KiotaSerializer.DeserializeAsync<T>(_jsonContentType, serializedRepresentation, cancellationToken);
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<T>(Stream stream, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
    => KiotaSerializer.DeserializeCollectionAsync(_jsonContentType, stream, parsableFactory, cancellationToken);
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="serializedRepresentation">The serialized representation of the objects.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<T>(string serializedRepresentation, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
    => KiotaSerializer.DeserializeCollectionAsync(_jsonContentType, serializedRepresentation, parsableFactory, cancellationToken);
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET5_0_OR_GREATER
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(Stream stream, CancellationToken cancellationToken = default) where T : IParsable
#else
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<T>(Stream stream, CancellationToken cancellationToken = default) where T : IParsable
#endif
    => KiotaSerializer.DeserializeCollectionAsync<T>(_jsonContentType, stream, cancellationToken);
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET5_0_OR_GREATER
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
#else
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<T>(string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
#endif
    => KiotaSerializer.DeserializeCollectionAsync<T>(_jsonContentType, serializedRepresentation, cancellationToken);
}