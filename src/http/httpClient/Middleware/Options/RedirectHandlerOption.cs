// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Net.Http;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options
{
    /// <summary>
    /// The redirect request option class
    /// </summary>
    public class RedirectHandlerOption : IRequestOption
    {
        private const int DefaultMaxRedirect = 5;
        private const int MaxMaxRedirect = 20;
        private int _maxRedirect = DefaultMaxRedirect;

        /// <summary>
        /// The maximum number of redirects with a maximum value of 20. This defaults to 5 redirects.
        /// </summary>
        public int MaxRedirect
        {
            get
            {
                return _maxRedirect;
            }
            set
            {
                if(value > MaxMaxRedirect)
                    throw new InvalidOperationException($"Maximum value for {nameof(MaxRedirect)} property exceeded ");

                _maxRedirect = value;
            }
        }

        /// <summary>
        /// A delegate that's called to determine whether a response should be redirected or not. The delegate method should accept <see cref="HttpResponseMessage"/> as it's parameter and return a <see cref="bool"/>. This defaults to true.
        /// </summary>
        public Func<HttpResponseMessage, bool> ShouldRedirect { get; set; } = (response) => true;

        /// <summary>
        /// A boolean value to determine if we redirects are allowed if the scheme changes(e.g. https to http). Defaults to false.
        /// </summary>
        public bool AllowRedirectOnSchemeChange { get; set; } = false;

        /// <summary>
        /// A callback that is invoked to scrub sensitive headers from the request before following a redirect.
        /// This callback receives the request being modified, the original URI, the new redirect URI, and a proxy resolver function.
        /// The proxy resolver returns the proxy URI for a given destination, or null if no proxy applies.
        /// Defaults to <see cref="DefaultScrubSensitiveHeaders"/>.
        /// </summary>
        public Action<HttpRequestMessage, Uri, Uri, Func<Uri, Uri?>?> ScrubSensitiveHeaders { get; set; } = DefaultScrubSensitiveHeaders;

        /// <summary>
        /// The default implementation for scrubbing sensitive headers during redirects.
        /// This method removes Authorization and Cookie headers when the host, scheme, or port changes,
        /// and removes ProxyAuthorization headers when no proxy is configured or the proxy is bypassed for the new URI.
        /// </summary>
        /// <param name="request">The HTTP request message to modify.</param>
        /// <param name="originalUri">The original request URI.</param>
        /// <param name="newUri">The new redirect URI.</param>
        /// <param name="proxyResolver">A function that returns the proxy URI for a destination, or null if no proxy applies. Can be null if no proxy is configured.</param>
        public static void DefaultScrubSensitiveHeaders(HttpRequestMessage request, Uri originalUri, Uri newUri, Func<Uri, Uri?>? proxyResolver)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            if(originalUri == null) throw new ArgumentNullException(nameof(originalUri));
            if(newUri == null) throw new ArgumentNullException(nameof(newUri));

            // Remove Authorization and Cookie headers if http request's scheme, host, or port changes
            var isDifferentOrigin = !newUri.Host.Equals(originalUri.Host, StringComparison.OrdinalIgnoreCase) ||
                !newUri.Scheme.Equals(originalUri.Scheme, StringComparison.OrdinalIgnoreCase) ||
                newUri.Port != originalUri.Port;
            if(isDifferentOrigin)
            {
                request.Headers.Authorization = null;
                request.Headers.Remove("Cookie");
            }

            // Remove ProxyAuthorization if no proxy is configured or the URL is bypassed
            var isProxyInactive = proxyResolver == null || proxyResolver(newUri) == null;
            if(isProxyInactive)
            {
                request.Headers.ProxyAuthorization = null;
            }
        }
    }
}
