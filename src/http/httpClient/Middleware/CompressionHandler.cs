// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation that handles compression.
    /// </summary>
    [Obsolete("kiota clients now rely on the HttpClientHandler to handle decompression")]
    public class CompressionHandler : DelegatingHandler
    {
        internal const string GZip = "gzip";

        /// <summary>
        /// Sends a HTTP request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request == null)
                throw new ArgumentNullException(nameof(request));

            Activity? activity;
            if(request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
            {
                var activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
                activity = activitySource?.StartActivity($"{nameof(CompressionHandler)}_{nameof(SendAsync)}");
                activity?.SetTag("com.microsoft.kiota.handler.compression.enable", true);
            }
            else
            {
                activity = null;
            }

            try
            {

                StringWithQualityHeaderValue gzipQHeaderValue = new StringWithQualityHeaderValue(GZip);

                // Add Accept-encoding: gzip header to incoming request if it doesn't have one.
                if(!request.Headers.AcceptEncoding.Contains(gzipQHeaderValue))
                {
                    request.Headers.AcceptEncoding.Add(gzipQHeaderValue);
                }

                HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                // Decompress response content when Content-Encoding: gzip header is present.
                if(ShouldDecompressContent(response))
                {
#if NET5_0_OR_GREATER
                    StreamContent streamContent = new StreamContent(new GZipStream(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), CompressionMode.Decompress));
#else
                    StreamContent streamContent = new StreamContent(new GZipStream(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), CompressionMode.Decompress));
#endif
                    // Copy Content Headers to the destination stream content
                    foreach(var httpContentHeader in response.Content.Headers)
                    {
                        streamContent.Headers.TryAddWithoutValidation(httpContentHeader.Key, httpContentHeader.Value);
                    }
                    response.Content = streamContent;
                }

                return response;
            }
            finally
            {
                activity?.Dispose();
            }
        }

        /// <summary>
        /// Checks if a <see cref="HttpResponseMessage"/> contains a Content-Encoding: gzip header.
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponseMessage"/> to check for header.</param>
        /// <returns></returns>
        private static bool ShouldDecompressContent(HttpResponseMessage httpResponse)
        {
            return httpResponse.Content?.Headers?.ContentEncoding.Contains(GZip) ?? false;
        }
    }
}
