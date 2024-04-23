// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    ///  This factory holds a list of all the registered factories for the various types of nodes.
    /// </summary>
    public class ParseNodeFactoryRegistry : IAsyncParseNodeFactory
    {
        /// <summary>
        /// The valid content type for the <see cref="ParseNodeFactoryRegistry"/>
        /// </summary>
        public string ValidContentType
        {
            get
            {
                throw new InvalidOperationException("The registry supports multiple content types. Get the registered factory instead.");
            }
        }
        /// <summary>
        /// Default singleton instance of the registry to be used when registering new factories that should be available by default.
        /// </summary>
        public static readonly ParseNodeFactoryRegistry DefaultInstance = new();
        /// <summary>
        /// List of factories that are registered by content type.
        /// </summary>
        public ConcurrentDictionary<string, IAsyncParseNodeFactory> ContentTypeAssociatedFactories { get; set; } = new();
        internal static readonly Regex contentTypeVendorCleanupRegex = new(@"[^/]+\+", RegexOptions.Compiled);

        /// <summary>
        /// Get the <see cref="IParseNode"/> instance that is the root of the content
        /// </summary>
        /// <param name="contentType">The content type of the stream</param>
        /// <param name="content">The <see cref="Stream"/> to parse</param>
        /// <returns></returns>
        [Obsolete("Use GetRootParseNodeAsync instead")]
        public IParseNode GetRootParseNode(string contentType, Stream content)
        {
            if(string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException(nameof(contentType));
            _ = content ?? throw new ArgumentNullException(nameof(content));

            var vendorSpecificContentType = contentType.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First();
            if(ContentTypeAssociatedFactories.TryGetValue(vendorSpecificContentType, out var vendorFactory))
                return vendorFactory.GetRootParseNode(vendorSpecificContentType, content);

            var cleanedContentType = contentTypeVendorCleanupRegex.Replace(vendorSpecificContentType, string.Empty);
            if(ContentTypeAssociatedFactories.TryGetValue(cleanedContentType, out var factory))
                return factory.GetRootParseNode(cleanedContentType, content);

            throw new InvalidOperationException($"Content type {cleanedContentType} does not have a factory registered to be parsed");
        }
        /// <summary>
        /// Get the <see cref="IParseNode"/> instance that is the root of the content
        /// </summary>
        /// <param name="contentType">The content type of the stream</param>
        /// <param name="content">The <see cref="Stream"/> to parse</param>
        /// <param name="cancellationToken">The cancellation token for the task</param>
        /// <returns></returns>
        public async Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content, 
            CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException(nameof(contentType));
            _ = content ?? throw new ArgumentNullException(nameof(content));

            var vendorSpecificContentType = contentType.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First();
            if(ContentTypeAssociatedFactories.TryGetValue(vendorSpecificContentType, out var vendorFactory))
                return await vendorFactory.GetRootParseNodeAsync(vendorSpecificContentType, content, cancellationToken).ConfigureAwait(false);

            var cleanedContentType = contentTypeVendorCleanupRegex.Replace(vendorSpecificContentType, string.Empty);
            if(ContentTypeAssociatedFactories.TryGetValue(cleanedContentType, out var factory))
                return await factory.GetRootParseNodeAsync(cleanedContentType, content, cancellationToken);

            throw new InvalidOperationException($"Content type {cleanedContentType} does not have a factory registered to be parsed");
        }
    }
}
