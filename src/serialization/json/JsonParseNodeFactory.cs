// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Json
{
    /// <summary>
    /// The <see cref="IParseNodeFactory"/> implementation for json content types
    /// </summary>
    public class JsonParseNodeFactory : IAsyncParseNodeFactory
    {
        private readonly KiotaJsonSerializationContext _jsonJsonSerializationContext;

        /// <summary>
        /// The <see cref="JsonParseNodeFactory"/> constructor.
        /// </summary>
        public JsonParseNodeFactory()
            : this(KiotaJsonSerializationContext.Default)
        {
        }

        /// <summary>
        /// The <see cref="JsonParseNodeFactory"/> constructor. 
        /// </summary>
        /// <param name="jsonJsonSerializationContext">The KiotaSerializationContext to utilize.</param>
        public JsonParseNodeFactory(KiotaJsonSerializationContext jsonJsonSerializationContext)
        {
            _jsonJsonSerializationContext = jsonJsonSerializationContext;
        }

        /// <summary>
        /// The valid content type for json
        /// </summary>
        public string ValidContentType { get; } = "application/json";

        /// <summary>
        /// Gets the root <see cref="IParseNode"/> of the json to be read.
        /// </summary>
        /// <param name="contentType">The content type of the stream to be parsed</param>
        /// <param name="content">The <see cref="Stream"/> containing json to parse.</param>
        /// <returns>An instance of <see cref="IParseNode"/> for json manipulation</returns> 
        [Obsolete("Use GetRootParseNodeAsync instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IParseNode GetRootParseNode(string contentType, Stream content)
        {
            if(string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException(nameof(contentType));
            else if(!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");

            _ = content ?? throw new ArgumentNullException(nameof(content));

            using var jsonDocument = JsonDocument.Parse(content);
            return new JsonParseNode(jsonDocument.RootElement.Clone(), _jsonJsonSerializationContext);
        }
        /// <summary>
        /// Asynchronously gets the root <see cref="IParseNode"/> of the json to be read.
        /// </summary>
        /// <param name="contentType">The content type of the stream to be parsed</param>
        /// <param name="content">The <see cref="Stream"/> containing json to parse.</param>
        /// <param name="cancellationToken">The cancellation token for the task</param>
        /// <returns>An instance of <see cref="IParseNode"/> for json manipulation</returns>
        public async Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content,
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException(nameof(contentType));
            else if(!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");

            _ = content ?? throw new ArgumentNullException(nameof(content));

            using var jsonDocument = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken).ConfigureAwait(false);
            return new JsonParseNode(jsonDocument.RootElement.Clone(), _jsonJsonSerializationContext);
        }
    }
}
