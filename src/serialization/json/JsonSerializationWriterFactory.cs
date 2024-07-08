// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Json
{
    /// <summary>
    /// The <see cref="ISerializationWriterFactory"/> implementation for the json content type
    /// </summary>
    public class JsonSerializationWriterFactory : ISerializationWriterFactory
    {
        private readonly KiotaJsonSerializationContext _kiotaJsonSerializationContext;

        /// <summary>
        /// The <see cref="JsonSerializationWriterFactory"/> constructor.
        /// </summary>
        public JsonSerializationWriterFactory()
            : this(KiotaJsonSerializationContext.Default)
        {
        }

        /// <summary>
        /// The <see cref="JsonSerializationWriterFactory"/> constructor.
        /// </summary>
        /// <param name="kiotaJsonSerializationContext">The KiotaJsonSerializationContext to use.</param>
        public JsonSerializationWriterFactory(KiotaJsonSerializationContext kiotaJsonSerializationContext)
        {
            _kiotaJsonSerializationContext = kiotaJsonSerializationContext;
        }
        
        /// <summary>
        /// The valid content type for json
        /// </summary>
        public string ValidContentType { get; } = "application/json";

        /// <summary>
        /// Get a valid <see cref="ISerializationWriter"/> for the content type
        /// </summary>
        /// <param name="contentType">The content type to search for</param>
        /// <returns>A <see cref="ISerializationWriter"/> instance for json writing</returns>
        public ISerializationWriter GetSerializationWriter(string contentType)
        {
            if(string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException(nameof(contentType));
            else if(!ValidContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentOutOfRangeException($"expected a {ValidContentType} content type");

            return new JsonSerializationWriter(_kiotaJsonSerializationContext);
        }
    }
}
