using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Defines the contract for a factory that creates parse nodes in an sync and async way.
    /// </summary>
    public interface IAsyncParseNodeFactory : IParseNodeFactory
    {
        /// <summary>
        /// Create a parse node from the given stream and content type.
        /// </summary>
        /// <param name="content">The stream to read the parse node from.</param>
        /// <param name="contentType">The content type of the parse node.</param>
        /// <param name="cancellationToken">The cancellation token for the task</param>
        /// <returns>A parse node.</returns>
        Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content, CancellationToken cancellationToken = default);
    }
}
