// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Extensions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Extensions
{
    /// <summary>
    /// Contains extension methods for <see cref="HttpRequestMessage"/>
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Gets a <see cref="IRequestOption"/> from <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> representation of the request.</param>
        /// <returns>A request option</returns>
        public static T? GetRequestOption<T>(this HttpRequestMessage httpRequestMessage) where T : IRequestOption
        {
#if NET5_0_OR_GREATER
            if(httpRequestMessage.Options.TryGetValue<T>(new HttpRequestOptionsKey<T>(typeof(T).FullName!), out var requestOption))
#else
            if(httpRequestMessage.Properties.TryGetValue(typeof(T).FullName!, out var requestOption))
#endif
            {
                return (T)requestOption!;
            }
            return default;
        }

        /// <summary>
        /// Create a new HTTP request by copying previous HTTP request's headers and properties from response's request message.
        /// </summary>
        /// <param name="originalRequest">The previous <see cref="HttpRequestMessage"/> needs to be copy.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>
        /// Re-issue a new HTTP request with the previous request's headers and properties
        /// </remarks>
        internal static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage originalRequest, CancellationToken cancellationToken = default)
        {
            var newRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri);

            // Copy request headers.
            foreach(var header in originalRequest.Headers)
                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);

            // Copy request properties.
#if NET5_0_OR_GREATER
            foreach(var property in originalRequest.Options)
                if(property.Value is IRequestOption requestOption)
                    newRequest.Options.Set(new HttpRequestOptionsKey<IRequestOption>(property.Key), requestOption);
                else
                    newRequest.Options.Set(new HttpRequestOptionsKey<object?>(property.Key), property.Value);
#else
            foreach(var property in originalRequest.Properties)
                IDictionaryExtensions.TryAdd(newRequest.Properties, property.Key, property.Value);
#endif

            // Set Content if previous request had one.
            if(originalRequest.Content != null)
            {
                // HttpClient doesn't rewind streams and we have to explicitly do so.
                var contentStream = new MemoryStream();
#if NET5_0_OR_GREATER
                await originalRequest.Content.CopyToAsync(contentStream, cancellationToken).ConfigureAwait(false);
#else
                await originalRequest.Content.CopyToAsync(contentStream).ConfigureAwait(false);
#endif

                if(contentStream.CanSeek)
                    contentStream.Seek(0, SeekOrigin.Begin);

                newRequest.Content = new StreamContent(contentStream);

                // Copy content headers.
                foreach(var header in originalRequest.Content.Headers)
                {
                    newRequest.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return newRequest;
        }

        /// <summary>
        /// Checks the HTTP request's content to determine if it's buffered or streamed content.
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <returns></returns>
        internal static bool IsBuffered(this HttpRequestMessage httpRequestMessage)
        {
            HttpContent? requestContent = httpRequestMessage.Content;

            if((httpRequestMessage.Method == HttpMethod.Put || httpRequestMessage.Method == HttpMethod.Post || httpRequestMessage.Method.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
               && requestContent != null && (requestContent.Headers.ContentLength == null || (int)requestContent.Headers.ContentLength == -1))
            {
                return false;
            }
            return true;
        }
    }
}
