using System;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Form;
/// <summary>Represents a serialization writer factory that can be used to create a form url encoded serialization writer.</summary>
public class FormSerializationWriterFactory : ISerializationWriterFactory
{
    /// <inheritdoc/>
    public string ValidContentType => "application/x-www-form-urlencoded";
    /// <inheritdoc/>
    public ISerializationWriter GetSerializationWriter(string contentType, bool serializeOnlyChangedValues = true)
    {
        if(string.IsNullOrEmpty(contentType))
            throw new ArgumentNullException(nameof(contentType));
        if(!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");
        return new FormSerializationWriter();
    }
}
