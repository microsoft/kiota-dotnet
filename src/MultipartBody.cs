// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Abstractions;

/// <summary>
/// Represents a multipart body for a request or a response.
/// </summary>
public class MultipartBody : IParsable
{
    private Lazy<string> _boundary = new Lazy<string>(() => Guid.NewGuid().ToString("N"));
    /// <summary>
    /// The boundary to use for the multipart body.
    /// </summary>
    public string Boundary => _boundary.Value;
    /// <summary>
    /// The request adapter to use for serialization.
    /// </summary>
    public IRequestAdapter? RequestAdapter { get; set; }
    /// <summary>
    /// Adds or replaces a part to the multipart body.
    /// </summary>
    /// <typeparam name="T">The type of the part value.</typeparam>
    /// <param name="partName">The name of the part.</param>
    /// <param name="contentType">The content type of the part.</param>
    /// <param name="partValue">The value of the part.</param>
    public void AddOrReplacePart<T>(string partName, string contentType, T partValue)
    {
        if(string.IsNullOrEmpty(partName))
        {
            throw new ArgumentNullException(nameof(partName));
        }
        if(string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentNullException(nameof(contentType));
        }
        if(partValue == null)
        {
            throw new ArgumentNullException(nameof(partValue));
        }
        var value = new Tuple<string, object>(contentType, partValue);
        if(!_parts.TryAdd(partName, value))
        {
            _parts[partName] = value;
        }
    }
    /// <summary>
    /// Gets the value of a part from the multipart body.
    /// </summary>
    /// <typeparam name="T">The type of the part value.</typeparam>
    /// <param name="partName">The name of the part.</param>
    /// <returns>The value of the part.</returns>
    public T? GetPartValue<T>(string partName)
    {
        if(string.IsNullOrEmpty(partName))
        {
            throw new ArgumentNullException(nameof(partName));
        }
        if(_parts.TryGetValue(partName, out var value))
        {
            return (T)value.Item2;
        }
        return default;
    }
    /// <summary>
    /// Removes a part from the multipart body.
    /// </summary>
    /// <param name="partName">The name of the part.</param>
    /// <returns>True if the part was removed, false otherwise.</returns>   
    public bool RemovePart(string partName)
    {
        if(string.IsNullOrEmpty(partName))
        {
            throw new ArgumentNullException(nameof(partName));
        }
        return _parts.Remove(partName);
    }
    private readonly Dictionary<string, Tuple<string, object>> _parts = new Dictionary<string, Tuple<string, object>>(StringComparer.OrdinalIgnoreCase);
    /// <inheritdoc />
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => throw new NotImplementedException();
    /// <inheritdoc />
    public void Serialize(ISerializationWriter writer)
    {
        if(writer == null)
        {
            throw new ArgumentNullException(nameof(writer));
        }
        if(RequestAdapter?.SerializationWriterFactory == null)
        {
            throw new InvalidOperationException(nameof(RequestAdapter.SerializationWriterFactory));
        }
        if(!_parts.Any())
        {
            throw new InvalidOperationException("No parts to serialize");
        }
        var first = true;
        foreach(var part in _parts)
        {
            try
            {
                if(first)
                    first = false;
                else
                    writer.WriteStringValue(string.Empty, string.Empty);

                writer.WriteStringValue(string.Empty, $"--{Boundary}");
                writer.WriteStringValue("Content-Type", $"{part.Value.Item1}");
                writer.WriteStringValue("Content-Disposition", $"form-data; name=\"{part.Key}\"");
                writer.WriteStringValue(string.Empty, string.Empty);
                if(part.Value.Item2 is IParsable parsable)
                {
                    using var partWriter = RequestAdapter.SerializationWriterFactory.GetSerializationWriter(part.Value.Item1);
                    partWriter.WriteObjectValue(string.Empty, parsable);
                    using var partContent = partWriter.GetSerializedContent();
                    if(partContent.CanSeek)
                        partContent.Seek(0, SeekOrigin.Begin);
                    using var ms = new MemoryStream();
                    partContent.CopyTo(ms);
                    writer.WriteByteArrayValue(string.Empty, ms.ToArray());
                }
                else if(part.Value.Item2 is string currentString)
                {
                    writer.WriteStringValue(string.Empty, currentString);
                }
                else if(part.Value.Item2 is Stream currentStream)
                {
                    if(currentStream.CanSeek)
                        currentStream.Seek(0, SeekOrigin.Begin);
                    using var ms = new MemoryStream();
                    currentStream.CopyTo(ms);
                    writer.WriteByteArrayValue(string.Empty, ms.ToArray());
                }
                else if(part.Value.Item2 is byte[] currentBinary)
                {
                    writer.WriteByteArrayValue(string.Empty, currentBinary);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported type {part.Value.Item2.GetType().Name} for part {part.Key}");
                }
            }
            catch(InvalidOperationException) when(part.Value.Item2 is byte[] currentBinary)
            { // binary payload
                writer.WriteByteArrayValue(part.Key, currentBinary);
            }
        }
        writer.WriteStringValue(string.Empty, string.Empty);
        writer.WriteStringValue(string.Empty, $"--{Boundary}--");
    }
}
