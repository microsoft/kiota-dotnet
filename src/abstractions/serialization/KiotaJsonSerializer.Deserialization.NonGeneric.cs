// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.\
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Abstractions.Serialization;
public static partial class KiotaJsonSerializer
{

    /// <summary>
    /// Deserializes the given string into an object.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
#if NET5_0_OR_GREATER
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType, string serializedRepresentation, CancellationToken cancellationToken = default)
#else
    public static Task<IParsable?> DeserializeAsync(Type targetType, string serializedRepresentation, CancellationToken cancellationToken = default)
#endif        
        => KiotaSerializer.DeserializeAsync(targetType, _jsonContentType, serializedRepresentation, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into an object.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
#if NET5_0_OR_GREATER
    public static Task<IParsable?> DeserializeAsync([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type targetType, Stream stream, CancellationToken cancellationToken = default)
#else
    public static Task<IParsable?> DeserializeAsync(Type targetType, Stream stream, CancellationToken cancellationToken = default)
#endif
         => KiotaSerializer.DeserializeAsync(targetType, _jsonContentType, stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type targetType, Stream stream, CancellationToken cancellationToken = default)
         => KiotaSerializer.DeserializeCollectionAsync(targetType, _jsonContentType, stream, cancellationToken);

    /// <summary>
    /// Deserializes the given stream into a collection of objects based on the content type.
    /// </summary>
    /// <param name="targetType">The target type to deserialize</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode("Activator creates an instance of a generic class with the Target Type as the generic type argument.")]
#endif
    public static Task<IEnumerable<IParsable>> DeserializeCollectionAsync(Type targetType, string serializedRepresentation, CancellationToken cancellationToken = default)
         => KiotaSerializer.DeserializeCollectionAsync(targetType, _jsonContentType, serializedRepresentation, cancellationToken);
}
