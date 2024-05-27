// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Abstractions;

/// <summary>
/// Represents a multipart body for a request or a response.
/// </summary>
public class MultipartBody : IParsable
{
    private readonly Lazy<string> _boundary = new Lazy<string>(() => Guid.NewGuid().ToString("N"));
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
    /// <param name="fileName">An optional file name for the part.</param>
    public void AddOrReplacePart<T>(string partName, string contentType, T partValue, string? fileName = null)
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
        var value = new Part(partName, partValue, contentType, fileName);
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
            if(value == null)
                return default;

            return (T)value.Content;
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

    private readonly Dictionary<string, Part> _parts = new Dictionary<string, Part>(StringComparer.OrdinalIgnoreCase);
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
        if(_parts.Count == 0)
        {
            throw new InvalidOperationException("No parts to serialize");
        }
        var first = true;
        var contentDispositionBuilder = new StringBuilder();
        foreach(var part in _parts.Values)
        {
            try
            {
                if(first)
                    first = false;
                else
                    AddNewLine(writer);

                writer.WriteStringValue(string.Empty, $"--{Boundary}");
                writer.WriteStringValue("Content-Type", part.ContentType);

                contentDispositionBuilder.Clear();
                contentDispositionBuilder.Append("form-data; name=\"");
                contentDispositionBuilder.Append(part.Name);
                contentDispositionBuilder.Append("\"");

                if(part.FileName != null)
                {
                    contentDispositionBuilder.Append("; filename=\"");
                    contentDispositionBuilder.Append(part.FileName);
                    contentDispositionBuilder.Append("\"");
                }

                writer.WriteStringValue("Content-Disposition", contentDispositionBuilder.ToString());

                AddNewLine(writer);
                if(part.Content is IParsable parsable)
                {
                    using var partWriter = RequestAdapter.SerializationWriterFactory.GetSerializationWriter(part.ContentType);
                    partWriter.WriteObjectValue(string.Empty, parsable);
                    using var partContent = partWriter.GetSerializedContent();
                    if(partContent.CanSeek)
                        partContent.Seek(0, SeekOrigin.Begin);
                    using var ms = new MemoryStream();
                    partContent.CopyTo(ms);
                    writer.WriteByteArrayValue(string.Empty, ms.ToArray());
                }
                else if(part.Content is string currentString)
                {
                    writer.WriteStringValue(string.Empty, currentString);
                }
                else if(part.Content is MemoryStream originalMemoryStream)
                {
                    writer.WriteByteArrayValue(string.Empty, originalMemoryStream.ToArray());
                }
                else if(part.Content is Stream currentStream)
                {
                    if(currentStream.CanSeek)
                        currentStream.Seek(0, SeekOrigin.Begin);
                    using var ms = new MemoryStream();
                    currentStream.CopyTo(ms);
                    writer.WriteByteArrayValue(string.Empty, ms.ToArray());
                }
                else if(part.Content is byte[] currentBinary)
                {
                    writer.WriteByteArrayValue(string.Empty, currentBinary);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported type {part.Content.GetType().Name} for part {part.Name}");
                }
            }
            catch(InvalidOperationException) when(part?.Content is byte[] currentBinary)
            { // binary payload
                writer.WriteByteArrayValue(part.Name, currentBinary);
            }
        }
        AddNewLine(writer);
        writer.WriteStringValue(string.Empty, $"--{Boundary}--");
    }
    private void AddNewLine(ISerializationWriter writer)
    {
        writer.WriteStringValue(string.Empty, string.Empty);
    }

    private class Part
    {
        public Part(string name, object content, string contentType, string? fileName)
        {
            this.Name = name;
            this.Content = content;
            this.ContentType = contentType;
            this.FileName = fileName;
        }

        public string Name { get; }
        public object Content { get; }
        public string ContentType { get; }
        public string? FileName { get; }
    }
}
