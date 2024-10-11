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

public static partial class KiotaJsonSerializer
{
    private static bool IsIParsable(this Type? type) => type?.IsAssignableTo(typeof(IParsable)) ?? false;
    private abstract class KiotaJsonDeserializer
    {
        private static readonly ConcurrentDictionary<Type, KiotaJsonDeserializer> _deserializers = new ConcurrentDictionary<Type, KiotaJsonDeserializer>();

        public static KiotaJsonDeserializer Create(Type type) => type.IsIParsable() ? _deserializers.GetOrAdd(type, CreateInternal) : throw new ArgumentException("The given Type is not of IParsable", nameof(type));

        private static KiotaJsonDeserializer CreateInternal([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType)
            => (KiotaJsonDeserializer)Activator.CreateInstance(typeof(TypedKiotaJsonDeserializer<>).MakeGenericType(targetType))!;


        internal abstract Task<IParsable?> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default);
        internal abstract Task<IParsable?> DeserializeAsync(string serializedRepresentation, CancellationToken cancellationToken = default);
        internal abstract Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Stream stream, CancellationToken cancellationToken = default);
        internal abstract Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string serializedRepresentation, CancellationToken cancellationToken = default);

        private class TypedKiotaJsonDeserializer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> : KiotaJsonDeserializer where T : IParsable
        {
            internal override async Task<IParsable?> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default) => await KiotaJsonSerializer.DeserializeAsync<T>(stream, cancellationToken);
            internal override async Task<IParsable?> DeserializeAsync(string serializedRepresentation, CancellationToken cancellationToken = default) => await KiotaJsonSerializer.DeserializeAsync<T>(serializedRepresentation, cancellationToken);
            internal override async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Stream stream, CancellationToken cancellationToken = default) => (await KiotaJsonSerializer.DeserializeCollectionAsync<T>(stream, cancellationToken)).OfType<IParsable>();
            internal override async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string serializedRepresentation, CancellationToken cancellationToken = default) => (await KiotaJsonSerializer.DeserializeCollectionAsync<T>(serializedRepresentation, cancellationToken)).OfType<IParsable>();
        }
    }

    /// <summary>
    /// Deserializes the given string into an object.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string serializedRepresentation, CancellationToken cancellationToken = default)
        => KiotaJsonDeserializer.Create(type).DeserializeAsync(serializedRepresentation, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into an object.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, Stream stream, CancellationToken cancellationToken = default)
         => KiotaJsonDeserializer.Create(type).DeserializeAsync(stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type type, Stream stream, CancellationToken cancellationToken = default)
         => KiotaJsonDeserializer.Create(type).DeserializeCollectionAsync(stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type type, string serializedRepresentation, CancellationToken cancellationToken = default)
         => KiotaJsonDeserializer.Create(type).DeserializeCollectionAsync(serializedRepresentation, cancellationToken);


}

#pragma warning restore IL3050
#endif