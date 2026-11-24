using System.Net;
#if !NET5_0_OR_GREATER
using System.Net.Http;
#endif
using System.Net.Http.Headers;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests
{
    public class KiotaClientFactoryTests
    {
        [Fact]
        public void ChainHandlersCollectionAndGetFirstLinkReturnsNullOnDefaultParams()
        {
            // Act
            var delegatingHandler = KiotaClientFactory.ChainHandlersCollectionAndGetFirstLink();
            // Assert
            Assert.Null(delegatingHandler);
        }

        [Fact]
        public void ChainHandlersCollectionAndGetFirstLinkWithSingleHandler()
        {
            // Arrange
            var handler = new TestHttpMessageHandler();
            // Act
            var delegatingHandler = KiotaClientFactory.ChainHandlersCollectionAndGetFirstLink(handler);
            // Assert
            Assert.NotNull(delegatingHandler);
            Assert.Null(delegatingHandler.InnerHandler);
        }

        [Fact]
        public void ChainHandlersCollectionAndGetFirstLinkWithMultipleHandlers()
        {
            // Arrange
            var handler1 = new TestHttpMessageHandler();
            var handler2 = new TestHttpMessageHandler();
            // Act
            var delegatingHandler = KiotaClientFactory.ChainHandlersCollectionAndGetFirstLink(handler1, handler2);
            // Assert
            Assert.NotNull(delegatingHandler);
            Assert.NotNull(delegatingHandler.InnerHandler); // first handler has an inner handler

            var innerHandler = delegatingHandler.InnerHandler as DelegatingHandler;
            Assert.NotNull(innerHandler);
            Assert.Null(innerHandler.InnerHandler);// end of the chain
        }

        [Fact]
        public void ChainHandlersCollectionAndGetFirstLinkWithMultipleHandlersSetsFinalHandler()
        {
            // Arrange
            var handler1 = new TestHttpMessageHandler();
            var handler2 = new TestHttpMessageHandler();
            var finalHandler = new HttpClientHandler();
            // Act
            var delegatingHandler = KiotaClientFactory.ChainHandlersCollectionAndGetFirstLink(finalHandler, handler1, handler2);
            // Assert
            Assert.NotNull(delegatingHandler);
            Assert.NotNull(delegatingHandler.InnerHandler); // first handler has an inner handler

            var innerHandler = delegatingHandler.InnerHandler as DelegatingHandler;
            Assert.NotNull(innerHandler);
            Assert.NotNull(innerHandler.InnerHandler);
            Assert.IsType<HttpClientHandler>(innerHandler.InnerHandler);
        }

        [Fact]
        public void GetDefaultHttpMessageHandlerEnablesMultipleHttp2Connections()
        {
            // Act
            var defaultHandler = KiotaClientFactory.GetDefaultHttpMessageHandler();
            // Assert
            Assert.NotNull(defaultHandler);
#if NETFRAMEWORK
            Assert.IsType<WinHttpHandler>(defaultHandler);
            Assert.True(((WinHttpHandler)defaultHandler).EnableMultipleHttp2Connections);
#else
            Assert.IsType<SocketsHttpHandler>(defaultHandler);
            Assert.True(((SocketsHttpHandler)defaultHandler).EnableMultipleHttp2Connections);
#endif
        }

        [Fact]
        public void GetDefaultHttpMessageHandlerLeavesRedirectsToMiddleware()
        {
            using var handler = KiotaClientFactory.GetDefaultHttpMessageHandler();
#if NETFRAMEWORK
            Assert.False(Assert.IsType<WinHttpHandler>(handler).AutomaticRedirection);
#else
            Assert.False(Assert.IsType<SocketsHttpHandler>(handler).AllowAutoRedirect);
#endif
        }

        [Fact]
        public void GetDefaultHttpMessageHandlerSetsUpProxy()
        {
            // Arrange
            var proxy = new WebProxy("http://localhost:8888", false);
            // Act
            var defaultHandler = KiotaClientFactory.GetDefaultHttpMessageHandler(proxy);
            // Assert
            Assert.NotNull(defaultHandler);
#if NETFRAMEWORK
            Assert.IsType<WinHttpHandler>(defaultHandler);
            Assert.Equal(proxy, ((WinHttpHandler)defaultHandler).Proxy);
#else
            Assert.IsType<SocketsHttpHandler>(defaultHandler);
            Assert.Equal(proxy, ((SocketsHttpHandler)defaultHandler).Proxy);
#endif
        }

        [Fact]
        public void CreateDefaultHandlersWithOptions()
        {
            // Arrange
            var retryHandlerOption = new RetryHandlerOption { MaxRetry = 5, ShouldRetry = (_, _, _) => true };

            // Act
            var handlers = KiotaClientFactory.CreateDefaultHandlers([retryHandlerOption]);
            var retryHandler = handlers.OfType<RetryHandler>().FirstOrDefault();

            // Assert
            Assert.NotNull(retryHandler);
            Assert.Equal(retryHandlerOption, retryHandler.RetryOption);
        }

        [Fact]
        public void DefaultHandlerListsUseTheSameOutboundOrder()
        {
            Type[] expected =
            [
                typeof(UriReplacementHandler<UriReplacementHandlerOption>),
                typeof(ParametersNameDecodingHandler),
                typeof(UserAgentHandler),
                typeof(RedirectHandler),
                typeof(RetryHandler),
                typeof(HeadersInspectionHandler),
                typeof(BodyInspectionHandler),
            ];
            Assert.Equal(expected, KiotaClientFactory.CreateDefaultHandlers().Select(handler => handler.GetType()));
#pragma warning disable CS0618 // Verify the legacy type list is still consistent.
            Assert.Equal(expected, KiotaClientFactory.GetDefaultHandlerTypes());
#pragma warning restore CS0618
            Assert.Equal(expected, KiotaClientFactory.GetDefaultHandlerActivatableTypes().Select(handler => handler.Type));
        }

        [Fact]
        public async Task RedirectHopsHaveSeparateRetryBudgetsAndKeepTheServerProvidedQuery()
        {
            using var initialFailure = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            using var redirect = new HttpResponseMessage(HttpStatusCode.Found);
            redirect.Headers.Location = new Uri("/second?%24select=id", UriKind.Relative);
            using var redirectFailure = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            using var success = new HttpResponseMessage(HttpStatusCode.OK);
            var transport = new ResponseSequenceHandler(initialFailure, redirect, redirectFailure, success);
            using var client = KiotaClientFactory.Create(transport, [new RetryHandlerOption { MaxRetry = 1, Delay = 0 }]);

            var response = await client.GetAsync("https://example.com/first?%24select=id", TestContext.Current.CancellationToken);

            Assert.Same(success, response);
            Assert.Equal(
                new (string, string?)[]
                {
                    ("/first?$select=id", null),
                    ("/first?$select=id", "1"),
                    ("/second?%24select=id", null),
                    ("/second?%24select=id", "1"),
                },
                transport.Requests.Select(request => (request.RequestUri!.PathAndQuery,
                    request.Headers.TryGetValues("Retry-Attempt", out var values) ? Assert.Single(values) : null)));
        }

        [Theory]
        [InlineData("https://example.com/second", true)]
        [InlineData("https://other.example.com/second", false)]
        [InlineData("https://example.com:8443/second", false)]
        [InlineData("http://example.com/second", false)]
        public async Task FactoryAuthorizationOnlyAnswersClaimsChallengesFromTheOriginalOrigin(string destination, bool sameOrigin)
        {
            var tokens = new Mock<IAccessTokenProvider>();
            tokens.SetupGet(provider => provider.AllowedHostsValidator).Returns(
                new AllowedHostsValidator(["example.com", "other.example.com"]));
            tokens.SetupSequence(provider => provider.GetAuthorizationTokenAsync(
                It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("initial-token").ReturnsAsync("claims-token");

            using var redirect = new HttpResponseMessage(HttpStatusCode.Found);
            redirect.Headers.Location = new Uri(destination);
            using var challenge = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            challenge.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", "error=\"insufficient_claims\", claims=\"YQ==\""));
            using var success = new HttpResponseMessage(HttpStatusCode.OK);
            var transport = new ResponseSequenceHandler(redirect, challenge, success);
            using var client = KiotaClientFactory.Create(new BaseBearerTokenAuthenticationProvider(tokens.Object),
                [new RedirectHandlerOption { AllowRedirectOnSchemeChange = true }], transport);

            var response = await client.GetAsync("https://example.com/first", TestContext.Current.CancellationToken);

            Assert.Same(sameOrigin ? success : challenge, response);
            string?[] expectedTokens = sameOrigin ? ["initial-token", "initial-token", "claims-token"] : ["initial-token", null];
            Assert.Equal(expectedTokens, transport.Requests.Select(request => request.Headers.Authorization?.Parameter));
            tokens.Verify(provider => provider.GetAuthorizationTokenAsync(
                It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()),
                Times.Exactly(sameOrigin ? 2 : 1));
        }

        private sealed class ResponseSequenceHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
        {
            public List<HttpRequestMessage> Requests { get; } = [];

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = responses[Requests.Count];
                Requests.Add(request);
                response.RequestMessage = request;
                return Task.FromResult(response);
            }
        }

        [Fact]
        public void CreateWithNullOrEmptyHandlersReturnsHttpClient()
        {
            var client = KiotaClientFactory.Create(null!, (HttpMessageHandler?)null);
            Assert.IsType<HttpClient>(client);

            client = KiotaClientFactory.Create(new List<DelegatingHandler>());
            Assert.IsType<HttpClient>(client);
        }

        [Fact]
        public void CreateWithCustomMiddlewarePipelineReturnsHttpClient()
        {
            var handlers = KiotaClientFactory.CreateDefaultHandlers();
#pragma warning disable CS0618 // Type or member is obsolete
            handlers.Add(new CompressionHandler());
#pragma warning restore CS0618 // Type or member is obsolete
            var client = KiotaClientFactory.Create(handlers);
            Assert.IsType<HttpClient>(client);
        }

        [Fact]
        public void CreateWithAuthenticationProvider()
        {
            var client = KiotaClientFactory.Create(new BaseBearerTokenAuthenticationProvider(new Mock<IAccessTokenProvider>().Object));
            Assert.IsType<HttpClient>(client);
        }
#if NET5_0_OR_GREATER

        [Fact]
        public void CreateReturnsHttpClientWithHttp2()
        {
            var client = KiotaClientFactory.Create();
            Assert.Equal(HttpVersion.Version20, client.DefaultRequestVersion);
        }
#endif
    }
}
