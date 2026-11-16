// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Net;
using System.Net.Http;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Microsoft.Kiota.Http.HttpClientLibrary
{
    /// <summary>
    /// This class is used to build the HttpClient instance used by the core service.
    /// </summary>
    public static class KiotaClientFactory
    {
        /// <summary>
        /// Initializes the <see cref="HttpClient"/> with the default configuration and middlewares including a authentication middleware using the <see cref="IAuthenticationProvider"/> if provided.
        /// </summary>
        /// <param name="finalHandler">The final <see cref="HttpMessageHandler"/> in the http pipeline. Can be configured for proxies, auto-decompression and auto-redirects </param>
        /// <param name="optionsForHandlers">A array of <see cref="IRequestOption"/> objects passed to the default handlers.</param>
        /// <returns>The <see cref="HttpClient"/> with the default middlewares.</returns>
        public static HttpClient Create(HttpMessageHandler? finalHandler = null, IRequestOption[]? optionsForHandlers = null)
            => Create(finalHandler, CreateDefaultHandlers(optionsForHandlers));

        /// <summary>
        /// Initializes the <see cref="HttpClient"/> with a custom middleware pipeline.
        /// </summary>
        /// <param name="handlers">The <see cref="DelegatingHandler"/> instances to create the <see cref="DelegatingHandler"/> from.</param>
        /// <param name="finalHandler">The final <see cref="HttpMessageHandler"/> in the http pipeline. Can be configured for proxies, auto-decompression and auto-redirects</param>
        /// <returns>The <see cref="HttpClient"/> with the custom handlers.</returns>
        public static HttpClient Create(IList<DelegatingHandler> handlers, HttpMessageHandler? finalHandler = null)
            => handlers?.Count is not > 0
                ? Create(finalHandler)
                : Create(finalHandler, handlers);

        /// <summary>
        /// Initializes the <see cref="HttpClient"/> with the default configuration and authentication middleware using the <see cref="IAuthenticationProvider"/> if provided.
        /// </summary>
        /// <param name="authenticationProvider"></param>
        /// <param name="optionsForHandlers"></param>
        /// <param name="finalHandler"></param>
        /// <returns></returns>
        public static HttpClient Create(BaseBearerTokenAuthenticationProvider authenticationProvider, IRequestOption[]? optionsForHandlers = null, HttpMessageHandler? finalHandler = null)
        {
            var defaultHandlersEnumerable = CreateDefaultHandlers(optionsForHandlers);
            defaultHandlersEnumerable.Add(new AuthorizationHandler(authenticationProvider));
            return Create(defaultHandlersEnumerable, finalHandler);
        }

        /// <summary>
        /// Creates a default set of middleware to be used by the <see cref="HttpClient"/>.
        /// </summary>
        /// <returns>A list of the default handlers used by the client.</returns>
        public static IList<DelegatingHandler> CreateDefaultHandlers(IRequestOption[]? optionsForHandlers = null)
        {
            optionsForHandlers ??= Array.Empty<IRequestOption>();

            UriReplacementHandlerOption? uriReplacementOption = null;
            RetryHandlerOption? retryHandlerOption = null;
            RedirectHandlerOption? redirectHandlerOption = null;
            ParametersNameDecodingOption? parametersNameDecodingOption = null;
            UserAgentHandlerOption? userAgentHandlerOption = null;
            HeadersInspectionHandlerOption? headersInspectionHandlerOption = null;
            BodyInspectionHandlerOption? bodyInspectionHandlerOption = null;

            foreach(var option in optionsForHandlers)
            {
                if(uriReplacementOption == null && option is UriReplacementHandlerOption uriOption)
                    uriReplacementOption = uriOption;
                else if(retryHandlerOption == null && option is RetryHandlerOption retryOption)
                    retryHandlerOption = retryOption;
                else if(redirectHandlerOption == null && option is RedirectHandlerOption redirectOption)
                    redirectHandlerOption = redirectOption;
                else if(parametersNameDecodingOption == null && option is ParametersNameDecodingOption parametersOption)
                    parametersNameDecodingOption = parametersOption;
                else if(userAgentHandlerOption == null && option is UserAgentHandlerOption userAgentOption)
                    userAgentHandlerOption = userAgentOption;
                else if(headersInspectionHandlerOption == null && option is HeadersInspectionHandlerOption headersInspectionOption)
                    headersInspectionHandlerOption = headersInspectionOption;
                else if(bodyInspectionHandlerOption == null && option is BodyInspectionHandlerOption bodyInspectionOption)
                    bodyInspectionHandlerOption = bodyInspectionOption;
            }

            return new List<DelegatingHandler>
            {
                uriReplacementOption != null ? new UriReplacementHandler<UriReplacementHandlerOption>(uriReplacementOption) : new UriReplacementHandler<UriReplacementHandlerOption>(),
                retryHandlerOption != null ? new RetryHandler(retryHandlerOption) : new RetryHandler(),
                redirectHandlerOption != null ? new RedirectHandler(redirectHandlerOption) : new RedirectHandler(),
                parametersNameDecodingOption != null ? new ParametersNameDecodingHandler(parametersNameDecodingOption) : new ParametersNameDecodingHandler(),
                userAgentHandlerOption != null ? new UserAgentHandler(userAgentHandlerOption) : new UserAgentHandler(),
                headersInspectionHandlerOption != null ? new HeadersInspectionHandler(headersInspectionHandlerOption) : new HeadersInspectionHandler(),
                bodyInspectionHandlerOption != null ? new BodyInspectionHandler(bodyInspectionHandlerOption) : new BodyInspectionHandler(),
            };
        }

        /// <summary>
        /// Gets the default handler types.
        /// </summary>
        /// <returns>A list of all the default handlers</returns>
        /// <remarks>Order matters</remarks>
        [Obsolete("Use GetDefaultHandlerActivatableTypes instead")]
        public static IList<System.Type> GetDefaultHandlerTypes()
        {
            return new List<System.Type>
            {
                typeof(UriReplacementHandler<UriReplacementHandlerOption>),
                typeof(RetryHandler),
                typeof(RedirectHandler),
                typeof(ParametersNameDecodingHandler),
                typeof(UserAgentHandler),
                typeof(HeadersInspectionHandler),
                typeof(BodyInspectionHandler),
            };
        }

        /// <summary>
        /// Gets the default handler types.
        /// </summary>
        /// <returns>A list of all the default handlers</returns>
        /// <remarks>Order matters</remarks>
        public static IList<ActivatableType> GetDefaultHandlerActivatableTypes()
        {
            return new List<ActivatableType>()
            {
                new(typeof(UriReplacementHandler<UriReplacementHandlerOption>)),
                new(typeof(RetryHandler)),
                new(typeof(RedirectHandler)),
                new(typeof(ParametersNameDecodingHandler)),
                new(typeof(UserAgentHandler)),
                new(typeof(HeadersInspectionHandler)),
                new(typeof(BodyInspectionHandler)),
            };
        }

        /// <summary>
        /// Provides DI-safe trim annotations for an underlying type.
        /// Required due to https://github.com/dotnet/runtime/issues/110239
        /// </summary>
        public readonly struct ActivatableType
        {
#if NET5_0_OR_GREATER
            /// <summary>
            /// Provides DI-safe trim annotations for an underlying type.
            /// </summary>
            /// <param name="type">The type to be wrapped.</param>
            public ActivatableType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
            {
                Type = type;
            }

            /// <summary>
            /// The underlying type.
            /// </summary>
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            public readonly Type Type;

#else
            /// <summary>
            /// Provides DI-safe trim annotations for an underlying type.
            /// </summary>
            /// <param name="type">The type to be wrapped.</param>
            public ActivatableType(Type type)
            {
                Type = type;
            }

            /// <summary>
            /// The underlying type.
            /// </summary>
            public readonly Type Type;
#endif

            /// <summary>
            /// Implicitly converts from the wrapper to the underlying type.
            /// </summary>
            /// <param name="type">An instance of <see cref="ActivatableType"/></param>
            /// <returns>The <see cref="Type"/></returns>
            public static implicit operator Type(ActivatableType type) => type.Type;
        }

        /// <summary>
        /// Creates a <see cref="DelegatingHandler"/> to use for the <see cref="HttpClient" /> from the provided <see cref="DelegatingHandler"/> instances. Order matters.
        /// </summary>
        /// <param name="finalHandler">The final <see cref="HttpMessageHandler"/> in the http pipeline. Can be configured for proxies, auto-decompression and auto-redirects </param>
        /// <param name="handlers">The <see cref="DelegatingHandler"/> instances to create the <see cref="DelegatingHandler"/> from.</param>
        /// <returns>The created <see cref="DelegatingHandler"/>.</returns>
        public static DelegatingHandler? ChainHandlersCollectionAndGetFirstLink(HttpMessageHandler? finalHandler, params DelegatingHandler[] handlers)
        {
            if(handlers == null || handlers.Length == 0) return default;
            var handlersCount = handlers.Length;
            for(var i = 0; i < handlersCount; i++)
            {
                var handler = handlers[i];
                var previousItemIndex = i - 1;
                if(previousItemIndex >= 0)
                {
                    var previousHandler = handlers[previousItemIndex];
                    previousHandler.InnerHandler = handler;
                }
            }
            if(finalHandler != null)
                handlers[handlers.Length - 1].InnerHandler = finalHandler;
            return handlers[0];//first
        }
        /// <summary>
        /// Creates a <see cref="DelegatingHandler"/> to use for the <see cref="HttpClient" /> from the provided <see cref="DelegatingHandler"/> instances. Order matters.
        /// </summary>
        /// <param name="handlers">The <see cref="DelegatingHandler"/> instances to create the <see cref="DelegatingHandler"/> from.</param>
        /// <returns>The created <see cref="DelegatingHandler"/>.</returns>
        public static DelegatingHandler? ChainHandlersCollectionAndGetFirstLink(params DelegatingHandler[] handlers)
        {
            return ChainHandlersCollectionAndGetFirstLink(null, handlers);
        }
        /// <summary>
        /// Gets a default Http Client handler with the appropriate proxy configurations
        /// </summary>
        /// <param name="proxy">The proxy to be used with created client.</param>
        /// <returns/>
        public static HttpMessageHandler GetDefaultHttpMessageHandler(IWebProxy? proxy = null)
        {
#if NETFRAMEWORK
            // If custom proxy is passed, the WindowsProxyUsePolicy will need updating
            // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http.WinHttpHandler/src/System/Net/Http/WinHttpHandler.cs#L575
            var proxyPolicy = proxy != null ? WindowsProxyUsePolicy.UseCustomProxy : WindowsProxyUsePolicy.UseWinHttpProxy;
            return new WinHttpHandler { Proxy = proxy, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate, WindowsProxyUsePolicy = proxyPolicy, SendTimeout = System.Threading.Timeout.InfiniteTimeSpan, ReceiveDataTimeout = System.Threading.Timeout.InfiniteTimeSpan, ReceiveHeadersTimeout = System.Threading.Timeout.InfiniteTimeSpan, EnableMultipleHttp2Connections = true };
#elif NET5_0_OR_GREATER && !BROWSER
            return new SocketsHttpHandler { Proxy = proxy, AllowAutoRedirect = false, EnableMultipleHttp2Connections = true, AutomaticDecompression = DecompressionMethods.All };
#elif BROWSER
            return new HttpClientHandler { AllowAutoRedirect = false };
#else
            return new HttpClientHandler { Proxy = proxy, AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
#endif
        }

        private static HttpClient Create(HttpMessageHandler? finalHandler, IList<DelegatingHandler> handlersArray)
        {
            var handler = ChainHandlersCollectionAndGetFirstLink(finalHandler ?? GetDefaultHttpMessageHandler(), [.. handlersArray]);
            var client = handler != null ? new HttpClient(handler) : new HttpClient();
#if NET5_0_OR_GREATER
            client.DefaultRequestVersion = HttpVersion.Version20;
#endif
            return client;
        }
    }
}
