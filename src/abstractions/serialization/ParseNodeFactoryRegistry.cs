// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
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
        public ConcurrentDictionary<string, IParseNodeFactory> ContentTypeAssociatedFactories { get; set; } = new();
        internal static readonly Regex contentTypeVendorCleanupRegex = new(@"[^/]+\+", RegexOptions.Compiled);

        /// <summary>
        /// Get the <see cref="IParseNode"/> instance that is the root of the content
        /// </summary>
        /// <param name="contentType">The content type of the stream</param>
        /// <param name="content">The <see cref="Stream"/> to parse</param>
        /// <returns></returns>
        [Obsolete("Use GetRootParseNodeAsync instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IParseNode GetRootParseNode(string contentType, Stream content)
        {
            if(string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException(nameof(contentType));
            _ = content ?? throw new ArgumentNullException(nameof(content));

            var (factory, correctContentType) = GetFactory<IParseNodeFactory>(contentType);
            return factory.GetRootParseNode(correctContentType, content);
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

            var (factory, correctContentType) = GetFactory<IAsyncParseNodeFactory>(contentType);

            return await factory.GetRootParseNodeAsync(correctContentType, content, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Get the <typeparamref name="T"/> instance for the given <paramref name="contentType"/>.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IParseNodeFactory"/>.</typeparam>
        /// <param name="contentType">The content type of the stream</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public (T, string ContentType) GetFactory<T>(string contentType)
            where T : IParseNodeFactory
        {
            string resultContentType;
            var vendorSpecificContentType = contentType.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
            IParseNodeFactory? factory;
            if(!ContentTypeAssociatedFactories.TryGetValue(vendorSpecificContentType, out factory))
            {
                var cleanedContentType = resultContentType = contentTypeVendorCleanupRegex.Replace(vendorSpecificContentType, string.Empty);
                if(!ContentTypeAssociatedFactories.TryGetValue(cleanedContentType, out factory))
                {
                    throw new InvalidOperationException($"Content type {cleanedContentType} does not have a factory registered to be parsed");
                }
                else
                {
                    resultContentType = cleanedContentType;
                }
            }
            else
            {
                resultContentType = vendorSpecificContentType;
            }

            if(factory is T typedFactory)
            {
                return (typedFactory, resultContentType);
            }
            throw new Exception($"{typeof(T).Name} factory is required");
        }
    }
}
