using Microsoft.Kiota.Abstractions.Serialization;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Serialization.Form;

/// <summary>
/// The <see cref="IAsyncParseNodeFactory"/> implementation for form content types
/// </summary>
public class FormParseNodeFactory : IAsyncParseNodeFactory
{
    /// <inheritdoc/>
    public string ValidContentType => "application/x-www-form-urlencoded";
    /// <inheritdoc/>
    public IParseNode GetRootParseNode(string contentType, Stream content) {
        if(string.IsNullOrEmpty(contentType))
            throw new ArgumentNullException(nameof(contentType));
        if(!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");
        if( content == null)
            throw new ArgumentNullException(nameof(content));

        using var reader = new StreamReader(content);
        var rawValue = reader.ReadToEnd();
        return new FormParseNode(rawValue);
    }
    /// <inheritdoc/>
    public async Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content, 
        CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrEmpty(contentType))
            throw new ArgumentNullException(nameof(contentType));
        if(!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");
        if(content == null)
            throw new ArgumentNullException(nameof(content));

        using var reader = new StreamReader(content);
        var rawValue = await reader.ReadToEndAsync().ConfigureAwait(false);
        return new FormParseNode(rawValue);
    }
}
