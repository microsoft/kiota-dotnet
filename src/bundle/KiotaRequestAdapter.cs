// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Form;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Kiota.Serialization.Multipart;
using Microsoft.Kiota.Serialization.Text;

namespace Microsoft.Kiota.Bundle
{
    /// <summary>
    /// The <see cref="IRequestAdapter"/> implementation that derived from <see cref="HttpClientRequestAdapter"/> with registrations configured.
    /// </summary>
    public class KiotaRequestAdapter : HttpClientRequestAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KiotaRequestAdapter"/> class.
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="parseNodeFactory">The parse node factory.</param>
        /// <param name="serializationWriterFactory">The serialization writer factory.</param>
        /// <param name="httpClient">The native HTTP client.</param>
        /// <param name="observabilityOptions">The observability options.</param>
        public KiotaRequestAdapter(IAuthenticationProvider authenticationProvider, IParseNodeFactory? parseNodeFactory = null, ISerializationWriterFactory? serializationWriterFactory = null, HttpClient? httpClient = null, ObservabilityOptions? observabilityOptions = null) : base(authenticationProvider, parseNodeFactory, serializationWriterFactory, httpClient, observabilityOptions)
        {
            SetupDefaults();
        }

        private static void SetupDefaults()
        {
            // Setup the default serializers/deserializers
            ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultSerializer<TextSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultSerializer<FormSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultSerializer<MultipartSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<TextParseNodeFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<FormParseNodeFactory>();
        }
    }
}
