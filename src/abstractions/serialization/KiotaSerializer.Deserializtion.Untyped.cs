// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------


#if NET5_0_OR_GREATER

using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable IL3050 

namespace Microsoft.Kiota.Abstractions.Serialization;

public static partial class KiotaSerializer
{
    private static bool IsIParsable(this Type? type) => type?.IsAssignableTo(typeof(IParsable)) ?? false;
    private abstract class KiotaDeserializer
    {
        private static readonly ConcurrentDictionary<Type, KiotaDeserializer> _deserializers = new ConcurrentDictionary<Type, KiotaDeserializer>();

        public static KiotaDeserializer Create(Type type) => type.IsIParsable() ? _deserializers.GetOrAdd(type, CreateInternal) : throw new ArgumentException("The given Type is not of IParsable", nameof(type));

        private static KiotaDeserializer CreateInternal([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType)
            => (KiotaDeserializer)Activator.CreateInstance(typeof(TypedKiotaDeserializer<>).MakeGenericType(targetType))!;


        internal abstract Task<IParsable?> DeserializeAsync(string contentType, Stream stream, CancellationToken cancellationToken = default);
        internal abstract Task<IParsable?> DeserializeAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default);
        internal abstract Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, Stream stream, CancellationToken cancellationToken = default);
        internal abstract Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default);

        private class TypedKiotaDeserializer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> : KiotaDeserializer where T : IParsable
        {
            internal override async Task<IParsable?> DeserializeAsync(string contentType, Stream stream, CancellationToken cancellationToken = default) => await KiotaSerializer.DeserializeAsync<T>(contentType, stream, cancellationToken);
            internal override async Task<IParsable?> DeserializeAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) => await KiotaSerializer.DeserializeAsync<T>(contentType, serializedRepresentation, cancellationToken);
            internal override async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, Stream stream, CancellationToken cancellationToken = default) => (await KiotaSerializer.DeserializeCollectionAsync<T>(contentType, stream, cancellationToken)).OfType<IParsable>();
            internal override async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) => (await KiotaSerializer.DeserializeCollectionAsync<T>(contentType, serializedRepresentation, cancellationToken)).OfType<IParsable>();
        }
    }

    /// <summary>
    /// Deserializes the given string into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    /// <returns></returns>
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string contentType, string serializedRepresentation, CancellationToken cancellationToken = default)
        => KiotaDeserializer.Create(type).DeserializeAsync(contentType, serializedRepresentation, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    /// <returns></returns>
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string contentType, Stream stream, CancellationToken cancellationToken = default)
         => KiotaDeserializer.Create(type).DeserializeAsync(contentType, stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type type, string contentType, Stream stream, CancellationToken cancellationToken = default)
         => KiotaDeserializer.Create(type).DeserializeCollectionAsync(contentType, stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type type, string contentType, string serializedRepresentation, CancellationToken cancellationToken = default)
         => KiotaDeserializer.Create(type).DeserializeCollectionAsync(contentType, serializedRepresentation, cancellationToken);

}

#pragma warning restore IL3050 
#endif