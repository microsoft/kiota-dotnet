using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Text;

/// <summary>
/// The <see cref="IParseNodeFactory"/> implementation for text/plain content types
/// </summary>
public class TextParseNodeFactory : IParseNodeFactory
{
    /// <inheritdoc />
    public string ValidContentType => "text/plain";
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
