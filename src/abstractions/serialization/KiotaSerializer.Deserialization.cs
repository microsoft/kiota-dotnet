// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif


namespace Microsoft.Kiota.Abstractions.Serialization;

public static partial class KiotaSerializer
{
    /// <summary>
    /// Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    [Obsolete("Use DeserializeAsync instead")]
    public static T? Deserialize<T>(string contentType, string serializedRepresentation, ParsableFactory<T> parsableFactory) where T : IParsable
    {
        if(string.IsNullOrEmpty(serializedRepresentation)) throw new ArgumentNullException(nameof(serializedRepresentation));
        using var stream = GetStreamFromString(serializedRepresentation);
        return Deserialize(contentType, stream, parsableFactory);
    }
    [Obsolete("Use GetStreamFromStringAsync instead")]
    private static Stream GetStreamFromString(string source)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);

        writer.Write(source);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    [Obsolete("Use DeserializeAsync instead")]
    public static T? Deserialize<T>(string contentType, Stream stream, ParsableFactory<T> parsableFactory) where T : IParsable
    {
        if(string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
        if(stream == null) throw new ArgumentNullException(nameof(stream));
        if(parsableFactory == null) throw new ArgumentNullException(nameof(parsableFactory));
        var parseNode = ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNode(contentType, stream);
        return parseNode.GetObjectValue(parsableFactory);
    }
    /// <summary>
    /// Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    [Obsolete("Use DeserializeAsync instead")]
#if NET5_0_OR_GREATER
    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, Stream stream) where T : IParsable
#else
    public static T? Deserialize<T>(string contentType, Stream stream) where T : IParsable
#endif
    => Deserialize(contentType, stream, GetFactoryFromType<T>());
#if NET5_0_OR_GREATER
    private static ParsableFactory<T> GetFactoryFromType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>() where T : IParsable
#else
    private static ParsableFactory<T> GetFactoryFromType<T>() where T : IParsable
#endif
    {
        var type = typeof(T);
        var factoryMethod = Array.Find(type.GetMethods(), static x => x.IsStatic && "CreateFromDiscriminatorValue".Equals(x.Name, StringComparison.OrdinalIgnoreCase)) ??
                            throw new InvalidOperationException($"No factory method found for type {type.Name}");
        return (ParsableFactory<T>)factoryMethod.CreateDelegate(typeof(ParsableFactory<T>));
    }
    /// <summary>
    /// Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    [Obsolete("Use DeserializeAsync instead")]
#if NET5_0_OR_GREATER
    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, string serializedRepresentation) where T : IParsable
#else
    public static T? Deserialize<T>(string contentType, string serializedRepresentation) where T : IParsable
#endif
    => Deserialize(contentType, serializedRepresentation, GetFactoryFromType<T>());

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    [Obsolete("Use DeserializeCollectionAsync instead")]
    public static IEnumerable<T> DeserializeCollection<T>(string contentType, Stream stream, ParsableFactory<T> parsableFactory) where T : IParsable
    {
        if(string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
        if(stream == null) throw new ArgumentNullException(nameof(stream));
        if(parsableFactory == null) throw new ArgumentNullException(nameof(parsableFactory));
        var parseNode = ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNode(contentType, stream);
        return parseNode.GetCollectionOfObjectValues(parsableFactory);
    }
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the objects.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    [Obsolete("Use DeserializeCollectionAsync instead")]
    public static IEnumerable<T> DeserializeCollection<T>(string contentType, string serializedRepresentation, ParsableFactory<T> parsableFactory) where T : IParsable
    {
        if(string.IsNullOrEmpty(serializedRepresentation)) throw new ArgumentNullException(nameof(serializedRepresentation));
        using var stream = GetStreamFromString(serializedRepresentation);
        return DeserializeCollection(contentType, stream, parsableFactory);
    }
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    [Obsolete("Use DeserializeCollectionAsync instead")]
#if NET5_0_OR_GREATER
    public static IEnumerable<T> DeserializeCollection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, Stream stream) where T : IParsable
#else
    public static IEnumerable<T> DeserializeCollection<T>(string contentType, Stream stream) where T : IParsable
#endif
    => DeserializeCollection(contentType, stream, GetFactoryFromType<T>());
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    [Obsolete("Use DeserializeCollectionAsync instead")]
#if NET5_0_OR_GREATER
    public static IEnumerable<T> DeserializeCollection<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, string serializedRepresentation) where T : IParsable
#else
    public static IEnumerable<T> DeserializeCollection<T>(string contentType, string serializedRepresentation) where T : IParsable
#endif
    => DeserializeCollection(contentType, serializedRepresentation, GetFactoryFromType<T>());

    /// <summary>
    /// Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static async Task<T?> DeserializeAsync<T>(string contentType, string serializedRepresentation, ParsableFactory<T> parsableFactory,
        CancellationToken cancellationToken = default) where T : IParsable
    {
        if(string.IsNullOrEmpty(serializedRepresentation)) throw new ArgumentNullException(nameof(serializedRepresentation));
        using var stream = await GetStreamFromStringAsync(serializedRepresentation).ConfigureAwait(false);
        return await DeserializeAsync(contentType, stream, parsableFactory, cancellationToken).ConfigureAwait(false);
    }
    private static async Task<Stream> GetStreamFromStringAsync(string source)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);

        // Some clients enforce async stream processing.
        await writer.WriteAsync(source).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static async Task<T?> DeserializeAsync<T>(string contentType, Stream stream, ParsableFactory<T> parsableFactory,
        CancellationToken cancellationToken = default) where T : IParsable
    {
        if(string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
        if(stream == null) throw new ArgumentNullException(nameof(stream));
        if(parsableFactory == null) throw new ArgumentNullException(nameof(parsableFactory));
        var parseNode = await ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNodeAsync(contentType, stream, cancellationToken).ConfigureAwait(false);
        return parseNode.GetObjectValue(parsableFactory);
    }

    /// <summary>
    /// Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET5_0_OR_GREATER
    public static Task<T?> DeserializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, Stream stream, CancellationToken cancellationToken = default) where T : IParsable
#else
    public static Task<T?> DeserializeAsync<T>(string contentType, Stream stream, CancellationToken cancellationToken = default) where T : IParsable
#endif
        => DeserializeAsync(contentType, stream, GetFactoryFromType<T>(), cancellationToken);
    /// <summary>
    /// Deserializes the given stream into an object based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET5_0_OR_GREATER
    public static Task<T?> DeserializeAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
#else
    public static Task<T?> DeserializeAsync<T>(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
#endif
    => DeserializeAsync(contentType, serializedRepresentation, GetFactoryFromType<T>(), cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static async Task<IEnumerable<T>> DeserializeCollectionAsync<T>(string contentType, Stream stream, ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
    {
        if(string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
        if(stream == null) throw new ArgumentNullException(nameof(stream));
        if(parsableFactory == null) throw new ArgumentNullException(nameof(parsableFactory));
        var parseNode = await ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNodeAsync(contentType, stream, cancellationToken);
        return parseNode.GetCollectionOfObjectValues(parsableFactory);
    }
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the objects.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static async Task<IEnumerable<T>> DeserializeCollectionAsync<T>(string contentType, string serializedRepresentation,
        ParsableFactory<T> parsableFactory, CancellationToken cancellationToken = default) where T : IParsable
    {
        if(string.IsNullOrEmpty(serializedRepresentation)) throw new ArgumentNullException(nameof(serializedRepresentation));
        using var stream = await GetStreamFromStringAsync(serializedRepresentation).ConfigureAwait(false);
        return await DeserializeCollectionAsync(contentType, stream, parsableFactory, cancellationToken).ConfigureAwait(false);
    }
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET5_0_OR_GREATER
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, Stream stream, CancellationToken cancellationToken = default) where T : IParsable
#else
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<T>(string contentType, Stream stream, CancellationToken cancellationToken = default) where T : IParsable
#endif
    => DeserializeCollectionAsync(contentType, stream, GetFactoryFromType<T>(), cancellationToken);
    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET5_0_OR_GREATER
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
#else
    public static Task<IEnumerable<T>> DeserializeCollectionAsync<T>(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) where T : IParsable
#endif
    => DeserializeCollectionAsync(contentType, serializedRepresentation, GetFactoryFromType<T>(), cancellationToken);
}