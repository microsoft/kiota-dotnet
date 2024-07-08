using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Text;

/// <summary>
/// The <see cref="IAsyncParseNodeFactory"/> implementation for text/plain content types
/// </summary>
public class TextParseNodeFactory : IAsyncParseNodeFactory
{
    /// <inheritdoc />
    public string ValidContentType => "text/plain";
    /// <inheritdoc />
    public IParseNode GetRootParseNode(string contentType, Stream content) {
        if(string.IsNullOrEmpty(contentType))
            throw new ArgumentNullException(nameof(contentType));
        else if(!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");
        
        _ = content ?? throw new ArgumentNullException(nameof(content));
        using var reader = new StreamReader(content);
        var stringContent = reader.ReadToEnd();
        return new TextParseNode(stringContent);
    }
    /// <inheritdoc />
    public async Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrEmpty(contentType))
            throw new ArgumentNullException(nameof(contentType));
        else if(!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");

        _ = content ?? throw new ArgumentNullException(nameof(content));

        using var reader = new StreamReader(content);
        var stringContent = await reader.ReadToEndAsync().ConfigureAwait(false);
        return new TextParseNode(stringContent);
    }
}
