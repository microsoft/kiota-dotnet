using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware
{
    public class UserAgentHandlerTests
    {
        private readonly HttpMessageInvoker _invoker;
        private readonly HttpClientRequestAdapter requestAdapter;
        public UserAgentHandlerTests()
        {
            var userAgentHandler = new UserAgentHandler
            {
                InnerHandler = new FakeSuccessHandler()
            };
            this._invoker = new HttpMessageInvoker(userAgentHandler);
            requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());
        }

        [Fact]
        public async Task DisabledUserAgentHandlerDoesNotChangeRequest()
        {
            // Arrange
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost"),
            };
            requestInfo.AddRequestOptions(new [] {
                new UserAgentHandlerOption
                {
                    Enabled = false
                }
            });
            // Act and get a request message
            var requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
            Assert.Empty(requestMessage.Headers);

            // Act
            var response = await _invoker.SendAsync(requestMessage, new CancellationToken());

            // Assert the request stays the same
            Assert.Empty(response.RequestMessage?.Headers!);
            Assert.Equal(requestMessage,response.RequestMessage);
        }
        [Fact]
        public async Task EnabledUserAgentHandlerAddsHeaderValue()
        {
            // Arrange
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost"),
            };
            var defaultOption = new UserAgentHandlerOption();
            // Act and get a request message
            var requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
            Assert.Empty(requestMessage.Headers);

            // Act
            var response = await _invoker.SendAsync(requestMessage, new CancellationToken());

            // Assert
            Assert.Single(response.RequestMessage?.Headers!);
            Assert.Single(response.RequestMessage?.Headers!.UserAgent);
            Assert.Equal(response.RequestMessage?.Headers!.UserAgent.First().Product.Name, defaultOption.ProductName, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(response.RequestMessage?.Headers!.UserAgent.First().Product.Version, defaultOption.ProductVersion, StringComparer.OrdinalIgnoreCase);
            Assert.Equal(response.RequestMessage?.Headers!.UserAgent.ToString(), $"{defaultOption.ProductName}/{defaultOption.ProductVersion}", StringComparer.OrdinalIgnoreCase);
            Assert.Equal(requestMessage,response.RequestMessage);
        }

        [Fact]
        public async Task DoesntAddProductTwice()
        {
            // Arrange
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost"),
            };
            var defaultOption = new UserAgentHandlerOption();
            // Act and get a request message
            var requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
            Assert.Empty(requestMessage.Headers);

            // Act
            var response = await _invoker.SendAsync(requestMessage, new CancellationToken());
            response = await _invoker.SendAsync(requestMessage, new CancellationToken());

            // Assert
            Assert.Single(response.RequestMessage?.Headers!);
            Assert.Single(response.RequestMessage?.Headers!.UserAgent);
            Assert.Equal(response.RequestMessage?.Headers!.UserAgent.ToString(), $"{defaultOption.ProductName}/{defaultOption.ProductVersion}", StringComparer.OrdinalIgnoreCase);
        }

    }

}
