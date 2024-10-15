// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------



using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;



#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Abstractions.Serialization;

public static partial class KiotaSerializer
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIParsable(this Type type) => typeof(IParsable).IsAssignableFrom(type);

    static internal class KiotaDeserializationWrapperFactory
    {
        private static readonly ConcurrentDictionary<Type, IKiotaDeserializationWrapper> _deserializers = new ConcurrentDictionary<Type, IKiotaDeserializationWrapper>();
#if NET7_0_OR_GREATER
        [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
        public static IKiotaDeserializationWrapper Create(Type type) => type.IsIParsable() ? _deserializers.GetOrAdd(type, CreateInternal) : throw new ArgumentException("The given Type is not of IParsable", nameof(type));

#if NET7_0_OR_GREATER
        [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
#if NET5_0_OR_GREATER
        private static IKiotaDeserializationWrapper CreateInternal([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType)
#else
        private static IKiotaDeserializationWrapper CreateInternal(Type targetType)
#endif
        {
            if(Activator.CreateInstance(typeof(KiotaDeserializationWrapper<>).MakeGenericType(targetType)) is IKiotaDeserializationWrapper deserializer)
                return deserializer;
            else
                throw new InvalidOperationException($"Unable to create deserializer for type {targetType}");
        }
    }

    internal interface IKiotaDeserializationWrapper
    {
        Task<IParsable?> DeserializeAsync(string contentType, Stream stream, CancellationToken cancellationToken = default);
        Task<IParsable?> DeserializeAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default);
        Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, Stream stream, CancellationToken cancellationToken = default);
        Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default);
    }
#if NET5_0_OR_GREATER
    internal class KiotaDeserializationWrapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> : IKiotaDeserializationWrapper where T : IParsable
#else
    internal class KiotaDeserializationWrapper<T> : IKiotaDeserializationWrapper where T : IParsable
#endif
    {
        public async Task<IParsable?> DeserializeAsync(string contentType, Stream stream, CancellationToken cancellationToken = default) => await KiotaSerializer.DeserializeAsync<T>(contentType, stream, cancellationToken);
        public async Task<IParsable?> DeserializeAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) => await KiotaSerializer.DeserializeAsync<T>(contentType, serializedRepresentation, cancellationToken);
        public async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, Stream stream, CancellationToken cancellationToken = default) => (await KiotaSerializer.DeserializeCollectionAsync<T>(contentType, stream, cancellationToken)).OfType<IParsable>();
        public async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string contentType, string serializedRepresentation, CancellationToken cancellationToken = default) => (await KiotaSerializer.DeserializeCollectionAsync<T>(contentType, serializedRepresentation, cancellationToken)).OfType<IParsable>();
    }

    /// <summary>
    /// Deserializes the given string into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    /// <returns></returns>

#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
#if NET5_0_OR_GREATER
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string contentType, string serializedRepresentation, CancellationToken cancellationToken = default)
#else
    public static Task<IParsable?> DeserializeAsync(Type type, string contentType, string serializedRepresentation, CancellationToken cancellationToken = default)
#endif
        => KiotaDeserializationWrapperFactory.Create(type).DeserializeAsync(contentType, serializedRepresentation, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    /// <returns></returns>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
#if NET5_0_OR_GREATER
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string contentType, Stream stream, CancellationToken cancellationToken = default)
#else
    public static Task<IParsable?> DeserializeAsync(Type type, string contentType, Stream stream, CancellationToken cancellationToken = default)
#endif
         => KiotaDeserializationWrapperFactory.Create(type).DeserializeAsync(contentType, stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type type, string contentType, Stream stream, CancellationToken cancellationToken = default)
         => KiotaDeserializationWrapperFactory.Create(type).DeserializeCollectionAsync(contentType, stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="type">The target type to deserialize</param>
    /// <param name="contentType">The content type of the stream.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type type, string contentType, string serializedRepresentation, CancellationToken cancellationToken = default)
         => KiotaDeserializationWrapperFactory.Create(type).DeserializeCollectionAsync(contentType, serializedRepresentation, cancellationToken);
}