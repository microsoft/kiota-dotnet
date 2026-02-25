// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation for handling redirection of requests.
    /// </summary>
    public class RedirectHandler : DelegatingHandler
    {
        /// <summary>
        /// Constructs a new <see cref="RedirectHandler"/>
        /// </summary>
        /// <param name="redirectOption">An OPTIONAL <see cref="RedirectHandlerOption"/> to configure <see cref="RedirectHandler"/></param>
        public RedirectHandler(RedirectHandlerOption? redirectOption = null)
        {
            RedirectOption = redirectOption ?? new RedirectHandlerOption();
        }

        /// <summary>
        /// RedirectOption property
        /// </summary>
        internal RedirectHandlerOption RedirectOption
        {
            get; private set;
        }

        /// <summary>
        /// Sends the Request and handles redirect responses if needed
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>for the request.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request == null) throw new ArgumentNullException(nameof(request));

            var redirectOption = request.GetRequestOption<RedirectHandlerOption>() ?? RedirectOption;

            ActivitySource? activitySource;
            Activity? activity;
            if(request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
            {
                activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
                activity = activitySource?.StartActivity($"{nameof(RedirectHandler)}_{nameof(SendAsync)}");
                activity?.SetTag("com.microsoft.kiota.handler.redirect.enable", true);
            }
            else
            {
                activity = null;
                activitySource = null;
            }
            try
            {

                // send request first time to get response
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                // check response status code and redirect handler option
                if(ShouldRedirect(response, redirectOption))
                {
                    if(response.Headers.Location == null)
                    {
                        throw new InvalidOperationException(
                            "Unable to perform redirect as Location Header is not set in response",
                                    new Exception($"No header present in response with status code {response.StatusCode}"));
                    }

                    var redirectCount = 0;

                    while(redirectCount < redirectOption.MaxRedirect)
                    {
                        using var redirectActivity = activitySource?.StartActivity($"{nameof(RedirectHandler)}_{nameof(SendAsync)} - redirect {redirectCount}");
                        redirectActivity?.SetTag("com.microsoft.kiota.handler.redirect.count", redirectCount);
                        redirectActivity?.SetTag("http.response.status_code", response.StatusCode);
                        // Drain response content to free responses.
                        if(response.Content != null)
                        {
#if NET5_0_OR_GREATER
                            await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
                            await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
                        }

                        // general clone request with internal CloneAsync (see CloneAsync for details) extension method
                        var originalRequest = response.RequestMessage;
                        if(originalRequest == null)
                        {
                            return response;// We can't clone the original request to replay it.
                        }
                        var newRequest = await originalRequest.CloneAsync(cancellationToken).ConfigureAwait(false);

                        // status code == 303: change request method from post to get and content to be null
                        if(response.StatusCode == HttpStatusCode.SeeOther)
                        {
                            newRequest.Content = null;
                            newRequest.Method = HttpMethod.Get;
                        }

                        // Set newRequestUri from response
                        if(response.Headers.Location?.IsAbsoluteUri ?? false)
                        {
                            newRequest.RequestUri = response.Headers.Location;
                        }
                        else
                        {
                            var baseAddress = newRequest.RequestUri?.GetComponents(UriComponents.SchemeAndServer | UriComponents.KeepDelimiter, UriFormat.Unescaped);
                            newRequest.RequestUri = new Uri(baseAddress + response.Headers.Location);
                        }

                        // Remove Authorization and Cookie header if http request's scheme or host changes
                        if(!newRequest.RequestUri.Host.Equals(request.RequestUri?.Host) ||
                        !newRequest.RequestUri.Scheme.Equals(request.RequestUri?.Scheme))
                        {
                            newRequest.Headers.Authorization = null;
                            newRequest.Headers.Remove("Cookie");
                        }

                        // Invoke the callback for every header to allow callers to strip additional headers
                        if(redirectOption.ShouldRemoveHeader != null)
                        {
                            var headersToRemove = newRequest.Headers
                                .Where(header => redirectOption.ShouldRemoveHeader(header.Key, newRequest.RequestUri, request.RequestUri))
                                .Select(header => header.Key)
                                .ToList();
                            foreach(var headerName in headersToRemove)
                                newRequest.Headers.Remove(headerName);
                        }

                        // Remove ProxyAuthorization if no proxy is configured or the URL is bypassed
                        var proxyResolver = GetProxyResolver();
                        if(proxyResolver == null || proxyResolver(newRequest.RequestUri) == null)
                        {
                            newRequest.Headers.ProxyAuthorization = null;
                        }

                        // If scheme has changed. Ensure that this has been opted in for security reasons
                        if(!newRequest.RequestUri.Scheme.Equals(request.RequestUri?.Scheme) && !redirectOption.AllowRedirectOnSchemeChange)
                        {
                            throw new InvalidOperationException(
                                $"Redirects with changing schemes not allowed by default. You can change this by modifying the {nameof(redirectOption.AllowRedirectOnSchemeChange)} option",
                                new Exception($"Scheme changed from {request.RequestUri?.Scheme} to {newRequest.RequestUri.Scheme}."));
                        }

                        // Send redirect request to get response
                        response = await base.SendAsync(newRequest, cancellationToken).ConfigureAwait(false);

                        // Check response status code
                        if(ShouldRedirect(response, redirectOption))
                        {
                            redirectCount++;
                        }
                        else
                        {
                            return response;
                        }
                    }

                    throw new InvalidOperationException(
                        "Too many redirects performed",
                        new Exception($"Max redirects exceeded. Redirect count : {redirectCount}"));
                }
                return response;
            }
            finally
            {
                activity?.Dispose();
            }
        }

        private bool ShouldRedirect(HttpResponseMessage responseMessage, RedirectHandlerOption redirectOption)
        {
            return IsRedirect(responseMessage.StatusCode) && redirectOption.ShouldRedirect(responseMessage) && redirectOption.MaxRedirect > 0;
        }

        /// <summary>
        /// Checks whether <see cref="HttpStatusCode"/> is redirected
        /// </summary>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <returns>Bool value for redirection or not</returns>
        private static bool IsRedirect(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.MovedPermanently => true,
                HttpStatusCode.Found => true,
                HttpStatusCode.SeeOther => true,
                HttpStatusCode.TemporaryRedirect => true,
                (HttpStatusCode)308 => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets a callback that resolves the proxy URI for a given destination URI.
        /// </summary>
        /// <returns>A function that takes a destination URI and returns the proxy URI, or null if no proxy is configured or the destination is bypassed.</returns>
        private Func<Uri, Uri?>? GetProxyResolver()
        {
            var proxy = GetProxyFromFinalHandler();
            if(proxy == null)
                return null;
            return destination => proxy.IsBypassed(destination) ? null : proxy.GetProxy(destination);
        }

        /// <summary>
        /// Traverses the handler chain to find the final handler and extract its proxy settings.
        /// </summary>
        /// <returns>The IWebProxy from the final handler, or null if not found.</returns>
        private IWebProxy? GetProxyFromFinalHandler()
        {
#if BROWSER
            // Browser platform does not support proxy configuration
            return null;
#else
            var handler = InnerHandler;
            while(handler != null)
            {
#if NETFRAMEWORK
                if(handler is WinHttpHandler winHttpHandler)
                    return winHttpHandler.Proxy;
#endif
#if NET5_0_OR_GREATER
                if(handler is SocketsHttpHandler socketsHandler)
                    return socketsHandler.Proxy;
#endif
                if(handler is HttpClientHandler httpClientHandler)
                    return httpClientHandler.Proxy;
                if(handler is DelegatingHandler delegatingHandler)
                    handler = delegatingHandler.InnerHandler;
                else
                    break;
            }
            return null;
#endif
        }
    }
}
