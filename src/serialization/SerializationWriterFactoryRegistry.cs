// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    ///  This factory holds a list of all the registered factories for the various types of nodes.
    /// </summary>
    public class SerializationWriterFactoryRegistry : ISerializationWriterFactory
    {
        /// <summary>
        /// The valid content type for the <see cref="SerializationWriterFactoryRegistry"/>
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
        public static readonly SerializationWriterFactoryRegistry DefaultInstance = new();
        /// <summary>
        /// List of factories that are registered by content type.
        /// </summary>
        public ConcurrentDictionary<string, ISerializationWriterFactory> ContentTypeAssociatedFactories { get; set; } = new();
        /// <summary>
        /// Get the relevant <see cref="ISerializationWriter"/> instance for the given content type
        /// </summary>
        /// <param name="contentType">The content type in use</param>
        /// <returns>A <see cref="ISerializationWriter"/> instance to parse the content</returns>
        public ISerializationWriter GetSerializationWriter(string contentType)
        {
            if(string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException(nameof(contentType));

            var vendorSpecificContentType = contentType.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).First();
            if(ContentTypeAssociatedFactories.ContainsKey(vendorSpecificContentType))
                return ContentTypeAssociatedFactories[vendorSpecificContentType].GetSerializationWriter(vendorSpecificContentType);

            var cleanedContentType = ParseNodeFactoryRegistry.contentTypeVendorCleanupRegex.Replace(vendorSpecificContentType, string.Empty);
            if(ContentTypeAssociatedFactories.ContainsKey(cleanedContentType))
                return ContentTypeAssociatedFactories[cleanedContentType].GetSerializationWriter(cleanedContentType);

            throw new InvalidOperationException($"Content type {cleanedContentType} does not have a factory registered to be parsed");
        }

    }
}
