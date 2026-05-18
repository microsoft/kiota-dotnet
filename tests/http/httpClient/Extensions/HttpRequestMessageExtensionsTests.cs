using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Extensions
{
    public class HttpRequestMessageExtensionsTests
    {
        private readonly HttpClientRequestAdapter requestAdapter;
        public HttpRequestMessageExtensionsTests()
        {
            requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());
        }
        [Fact]
        public async Task GetRequestOptionCanExtractRequestOptionFromHttpRequestMessage()
        {
            // Arrange
            var requestInfo = new RequestInformation()
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };
            var redirectHandlerOption = new RedirectHandlerOption()
            {
                MaxRedirect = 7
            };
            requestInfo.AddRequestOptions(new IRequestOption[] { redirectHandlerOption });
            // Act and get a request message
            var requestMessage = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo, TestContext.Current.CancellationToken);
            Assert.NotNull(requestMessage);

            var extractedOption = requestMessage.GetRequestOption<RedirectHandlerOption>();
            // Assert
            Assert.NotNull(extractedOption);
            Assert.Equal(redirectHandlerOption, extractedOption);
            Assert.Equal(7, redirectHandlerOption.MaxRedirect);
        }

        [Fact]
        public async Task CloneAsyncWithEmptyHttpContent()
        {
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };
            var originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo, TestContext.Current.CancellationToken);
            Assert.NotNull(originalRequest);

            HttpRequestMessage clonedRequest = await originalRequest.CloneAsync(TestContext.Current.CancellationToken);

            Assert.NotNull(clonedRequest);
            Assert.Equal(originalRequest.Method, clonedRequest.Method);
            Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
            Assert.Null(clonedRequest.Content);
        }

        [Fact]
        public async Task CloneAsyncWithHttpContent()
        {
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };
            var originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo, TestContext.Current.CancellationToken);
            Assert.NotNull(originalRequest);

            originalRequest.Content = new StringContent("contents");

            var clonedRequest = await originalRequest.CloneAsync(TestContext.Current.CancellationToken);
            var originalContents = await originalRequest.Content.ReadAsStringAsync(
#if NET5_0_OR_GREATER
                TestContext.Current.CancellationToken
#endif
            );
            var clonedRequestContents = await clonedRequest.Content!.ReadAsStringAsync(
#if NET5_0_OR_GREATER
                TestContext.Current.CancellationToken
#endif
            );

            Assert.NotNull(clonedRequest);
            Assert.Equal(originalRequest.Method, clonedRequest.Method);
            Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
            Assert.Equal(originalContents, clonedRequestContents);
            Assert.Equal(originalRequest.Content?.Headers.ContentType, clonedRequest.Content?.Headers.ContentType);
        }

        [Fact]
        public async Task CloneAsyncWithHttpStreamContent()
        {
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };
            requestInfo.SetStreamContent(new MemoryStream(Encoding.UTF8.GetBytes("contents")), "application/octet-stream");
            var originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo, TestContext.Current.CancellationToken);
            Assert.NotNull(originalRequest);

            var clonedRequest = await originalRequest.CloneAsync(TestContext.Current.CancellationToken);
            var originalContents = await originalRequest.Content!.ReadAsStringAsync(
#if NET5_0_OR_GREATER
                TestContext.Current.CancellationToken
#endif
            );
            var clonedRequestContents = await clonedRequest.Content!.ReadAsStringAsync(
#if NET5_0_OR_GREATER
                TestContext.Current.CancellationToken
#endif
            );

            Assert.NotNull(clonedRequest);
            Assert.Equal(originalRequest.Method, clonedRequest.Method);
            Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
            Assert.Equal(originalContents, clonedRequestContents);
            Assert.Equal(originalRequest.Content?.Headers.ContentType, clonedRequest.Content?.Headers.ContentType);
        }

        [Fact]
        public async Task CloneAsyncWithRequestOption()
        {
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };
            var redirectHandlerOption = new RedirectHandlerOption()
            {
                MaxRedirect = 7
            };
            requestInfo.AddRequestOptions(new IRequestOption[] { redirectHandlerOption });
            var originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo, TestContext.Current.CancellationToken);
            Assert.NotNull(originalRequest);

            originalRequest.Content = new StringContent("contents");

            var clonedRequest = await originalRequest.CloneAsync(TestContext.Current.CancellationToken);

            Assert.NotNull(clonedRequest);
            Assert.Equal(originalRequest.Method, clonedRequest.Method);
            Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
#if NET5_0_OR_GREATER
            Assert.NotEmpty(clonedRequest.Options);
            Assert.Equal(redirectHandlerOption, clonedRequest.Options.First().Value);
#else
            Assert.NotEmpty(clonedRequest.Properties);
            Assert.Equal(redirectHandlerOption, clonedRequest.Properties.First().Value);
#endif
            Assert.Equal(originalRequest.Content?.Headers.ContentType, clonedRequest.Content?.Headers.ContentType);
        }

        [Fact]
        public async Task IsBufferedReturnsTrueForGetRequest()
        {
            // Arrange
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };
            var originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo, TestContext.Current.CancellationToken);
            Assert.NotNull(originalRequest);

            // Act
            var response = originalRequest.IsBuffered();
            // Assert
            Assert.True(response, "Unexpected content type");
        }
        [Fact]
        public async Task IsBufferedReturnsTrueForPostWithNoContent()
        {
            // Arrange
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.POST,
                URI = new Uri("http://localhost")
            };
            var originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo, TestContext.Current.CancellationToken);
            Assert.NotNull(originalRequest);

            // Act
            var response = originalRequest.IsBuffered();
            // Assert
            Assert.True(response, "Unexpected content type");
        }
        [Fact]
        public async Task IsBufferedReturnsTrueForPostWithBufferStringContent()
        {
            // Arrange
            byte[] data = new byte[] { 1, 2, 3, 4, 5 };
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.POST,
                URI = new Uri("http://localhost"),
                Content = new MemoryStream(data)
            };
            var originalRequest = await requestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo, TestContext.Current.CancellationToken);
            Assert.NotNull(originalRequest);

            // Act
            var response = originalRequest.IsBuffered();
            // Assert
            Assert.True(response, "Unexpected content type");
        }
    }
}
