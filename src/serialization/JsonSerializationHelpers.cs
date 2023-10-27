// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.IO;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Abstractions.Serialization;

/// <summary>
/// Set of helper methods for JSON serialization
/// </summary>
public class JsonSerializationHelpers
{
    private const string _jsonContentType = "application/json";
    /// <summary>
    /// Deserializes the given stream into a object.
    /// </summary>
    /// <param name="parsableFactory">The factory to create the object.</param>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
    public static T Deserialize<T>(string serializedRepresentation, ParsableFactory<T> parsableFactory) where T : IParsable
    => SerializationHelpers.Deserialize(_jsonContentType, serializedRepresentation, parsableFactory);
    /// <summary>
    /// Deserializes the given stream into a object based on the content type.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    /// <param name="parsableFactory">The factory to create the object.</param>
    public static T Deserialize<T>(Stream stream, ParsableFactory<T> parsableFactory) where T : IParsable
    => SerializationHelpers.Deserialize(_jsonContentType, stream, parsableFactory);
    /// <summary>
    /// Deserializes the given stream into a object based on the content type.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
#if NET5_0_OR_GREATER
    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(Stream stream) where T : IParsable
#else
    public static T Deserialize<T>(Stream stream) where T : IParsable
#endif
    => SerializationHelpers.Deserialize<T>(_jsonContentType, stream);
    /// <summary>
    /// Deserializes the given stream into a object based on the content type.
    /// </summary>
    /// <param name="serializedRepresentation">The serialized representation of the object.</param>
#if NET5_0_OR_GREATER
    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] T>(string serializedRepresentation) where T : IParsable
#else
    public static T Deserialize<T>(string serializedRepresentation) where T : IParsable
#endif
    => SerializationHelpers.Deserialize<T>(_jsonContentType, serializedRepresentation);
}