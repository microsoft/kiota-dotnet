// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.\
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

namespace Microsoft.Kiota.Abstractions.Serialization;
public static partial class KiotaJsonSerializer
{
#if NET8_0_OR_GREATER
#else
    /// <summary>
    /// Palceholder for .NET &lt; 8
    /// </summary>
    private class RequiresDynamicCodeAttribute : Attribute
    {
        public RequiresDynamicCodeAttribute(string _) { }
    }

#endif
    private static bool IsIParsable(this Type type) => type.IsAssignableTo(typeof(IParsable));

    private static class KiotaJsonDeserializationWrapperFactory
    {
        private static readonly ConcurrentDictionary<Type, IKiotaJsonDeserializationWrapper> _deserializers = new ConcurrentDictionary<Type, IKiotaJsonDeserializationWrapper>();

        [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
        public static IKiotaJsonDeserializationWrapper Create(Type type) => type.IsIParsable() ? _deserializers.GetOrAdd(type, CreateInternal) : throw new ArgumentException("The given Type is not of IParsable", nameof(type));

        [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
        private static IKiotaJsonDeserializationWrapper CreateInternal([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType)
        {
            if(Activator.CreateInstance(typeof(KiotaJsonDeserializationWrapper<>).MakeGenericType(targetType)) is IKiotaJsonDeserializationWrapper deserializer)
                return deserializer;
            else
                throw new InvalidOperationException($"Unable to create deserializer for type {targetType}");
        }
    }

    private interface IKiotaJsonDeserializationWrapper
    {
        Task<IParsable?> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default);
        Task<IParsable?> DeserializeAsync(string serializedRepresentation, CancellationToken cancellationToken = default);
        Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Stream stream, CancellationToken cancellationToken = default);
        Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string serializedRepresentation, CancellationToken cancellationToken = default);
    }

    private class KiotaJsonDeserializationWrapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T> : IKiotaJsonDeserializationWrapper where T : IParsable
    {
        public async Task<IParsable?> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default) => await KiotaJsonSerializer.DeserializeAsync<T>(stream, cancellationToken);
        public async Task<IParsable?> DeserializeAsync(string serializedRepresentation, CancellationToken cancellationToken = default) => await KiotaJsonSerializer.DeserializeAsync<T>(serializedRepresentation, cancellationToken);
        public async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Stream stream, CancellationToken cancellationToken = default) => (await KiotaJsonSerializer.DeserializeCollectionAsync<T>(stream, cancellationToken)).OfType<IParsable>();
        public async Task<IEnumerable<IParsable>> DeserializeCollectionAsync(string serializedRepresentation, CancellationToken cancellationToken = default) => (await KiotaJsonSerializer.DeserializeCollectionAsync<T>(serializedRepresentation, cancellationToken)).OfType<IParsable>();
    }

    /// <summary>
    /// Deserializes the given string into an object.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType, string serializedRepresentation, CancellationToken cancellationToken = default)
        => KiotaJsonDeserializationWrapperFactory.Create(targetType).DeserializeAsync(serializedRepresentation, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into an object.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType, Stream stream, CancellationToken cancellationToken = default)
         => KiotaJsonDeserializationWrapperFactory.Create(targetType).DeserializeAsync(stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type targetType, Stream stream, CancellationToken cancellationToken = default)
         => KiotaJsonDeserializationWrapperFactory.Create(targetType).DeserializeCollectionAsync(stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type targetType, string serializedRepresentation, CancellationToken cancellationToken = default)
         => KiotaJsonDeserializationWrapperFactory.Create(targetType).DeserializeCollectionAsync(serializedRepresentation, cancellationToken);
}
#endif