using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
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
        public void CreateWithNullOrEmptyHandlersReturnsHttpClient()
        {
            var client = KiotaClientFactory.Create(null, (HttpMessageHandler)null);
            Assert.IsType<HttpClient>(client);

            client = KiotaClientFactory.Create(new List<DelegatingHandler>());
            Assert.IsType<HttpClient>(client);
        }

        [Fact]
        public void CreateWithCustomMiddlewarePipelineReturnsHttpClient()
        {
            var handlers = KiotaClientFactory.CreateDefaultHandlers();
            handlers.Add(new CompressionHandler());
            var client = KiotaClientFactory.Create(handlers);
            Assert.IsType<HttpClient>(client);
        }
    }
}
