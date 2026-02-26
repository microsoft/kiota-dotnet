using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware
{
    /// <summary>
    /// A mock IWebProxy implementation for testing proxy bypass scenarios.
    /// </summary>
    internal class MockWebProxy : IWebProxy
    {
        private readonly Uri _proxyUri;
        private readonly string[] _bypassList;

        public MockWebProxy(Uri proxyUri, params string[] bypassList)
        {
            _proxyUri = proxyUri;
            _bypassList = bypassList;
        }

        public ICredentials? Credentials { get; set; }

        public Uri? GetProxy(Uri destination) => _proxyUri;

        public bool IsBypassed(Uri host)
        {
            return _bypassList.Any(bypass => host.Host.IndexOf(bypass, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    /// <summary>
    /// A mock DelegatingHandler for testing that allows setting responses and proper chaining.
    /// </summary>
    internal class MockDelegatingRedirectHandler : DelegatingHandler
    {
        private HttpResponseMessage? _response1;
        private HttpResponseMessage? _response2;
        private bool _response1Sent;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(!_response1Sent)
            {
                _response1Sent = true;
                _response1!.RequestMessage = request;
                return Task.FromResult(_response1);
            }
            else
            {
                _response1Sent = false;
                _response2!.RequestMessage = request;
                return Task.FromResult(_response2);
            }
        }

        public void SetHttpResponse(HttpResponseMessage? response1, HttpResponseMessage? response2 = null)
        {
            _response1Sent = false;
            _response1 = response1;
            _response2 = response2;
        }
    }

    public sealed class RedirectHandlerTests : IDisposable
    {
        private readonly MockRedirectHandler _testHttpMessageHandler;
        private readonly RedirectHandler _redirectHandler;
        private readonly HttpMessageInvoker _invoker;

        public RedirectHandlerTests()
        {
            this._testHttpMessageHandler = new MockRedirectHandler();
            this._redirectHandler = new RedirectHandler
            {
                InnerHandler = _testHttpMessageHandler
            };
            this._invoker = new HttpMessageInvoker(this._redirectHandler);
        }

        public void Dispose()
        {
            this._invoker.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void RedirectHandler_Constructor()
        {
            // Assert
            using RedirectHandler redirect = new RedirectHandler();
            Assert.Null(redirect.InnerHandler);
            Assert.NotNull(redirect.RedirectOption);
            Assert.Equal(5, redirect.RedirectOption.MaxRedirect); // default MaxRedirects is 5
            Assert.IsType<RedirectHandler>(redirect);
        }

        [Fact]
        public void RedirectHandler_HttpMessageHandlerConstructor()
        {
            // Assert
            Assert.NotNull(this._redirectHandler.InnerHandler);
            Assert.NotNull(_redirectHandler.RedirectOption);
            Assert.Equal(5, _redirectHandler.RedirectOption.MaxRedirect); // default MaxRedirects is 5
            Assert.Equal(this._redirectHandler.InnerHandler, this._testHttpMessageHandler);
            Assert.IsType<RedirectHandler>(this._redirectHandler);
        }

        [Fact]
        public void RedirectHandler_RedirectOptionConstructor()
        {
            // Assert
            using RedirectHandler redirect = new RedirectHandler(new RedirectHandlerOption { MaxRedirect = 2, AllowRedirectOnSchemeChange = true });
            Assert.Null(redirect.InnerHandler);
            Assert.NotNull(redirect.RedirectOption);
            Assert.Equal(2, redirect.RedirectOption.MaxRedirect);
            Assert.True(redirect.RedirectOption.AllowRedirectOnSchemeChange);
            Assert.IsType<RedirectHandler>(redirect);
        }

        [Fact]
        public async Task OkStatusShouldPassThrough()
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo"))
            {
                // Arrange
                var redirectResponse = new HttpResponseMessage(HttpStatusCode.OK);
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse); // sets the mock response
                // Act
                var response = await this._invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Same(response.RequestMessage, httpRequestMessage);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308 not available in netstandard
        public async Task ShouldRedirectSameMethodAndContent(HttpStatusCode statusCode)
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo"))
            {
                // Arrange
                httpRequestMessage.Content = new StringContent("Hello World");

                var redirectResponse = new HttpResponseMessage(statusCode);
                redirectResponse.Headers.Location = new Uri("http://example.org/bar");
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.Equal(response.RequestMessage?.Method, httpRequestMessage.Method);
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.NotNull(response.RequestMessage?.Content);
                Assert.Equal("Hello World", await response.RequestMessage.Content.ReadAsStringAsync(
#if NET5_0_OR_GREATER
                  TestContext.Current.CancellationToken
#endif
                ));
            }
        }

        [Fact]
        public async Task ShouldRedirectChangeMethodAndContent()
        {

            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo"))
            {
                // Arrange
                httpRequestMessage.Content = new StringContent("Hello World");

                var redirectResponse = new HttpResponseMessage(HttpStatusCode.SeeOther);
                redirectResponse.Headers.Location = new Uri("http://example.org/bar");
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.NotEqual(response.RequestMessage?.Method, httpRequestMessage.Method);
                Assert.Equal(response.RequestMessage?.Method, HttpMethod.Get);
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.Null(response.RequestMessage?.Content);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectWithDifferentHostShouldRemoveAuthHeader(HttpStatusCode statusCode)
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo"))
            {
                // Arrange
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");
                var redirectResponse = new HttpResponseMessage(statusCode);
                redirectResponse.Headers.Location = new Uri("http://example.net/bar");
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.NotSame(response.RequestMessage?.RequestUri?.Host, httpRequestMessage.RequestUri?.Host);
                Assert.Null(response.RequestMessage?.Headers.Authorization);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectWithDifferentSchemeThrowsInvalidOperationExceptionIfAllowRedirectOnSchemeChangeIsDisabled(HttpStatusCode statusCode)
        {

            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.org/foo"))
            {
                // Arrange
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");
                var redirectResponse = new HttpResponseMessage(statusCode);
                redirectResponse.Headers.Location = new Uri("http://example.org/bar");
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => this._invoker.SendAsync(httpRequestMessage, CancellationToken.None));
                // Assert
                Assert.Contains("Redirects with changing schemes not allowed by default", exception.Message);
                Assert.Equal("Scheme changed from https to http.", exception.InnerException?.Message);
                Assert.IsType<InvalidOperationException>(exception);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectWithDifferentSchemeShouldRemoveAuthHeaderIfAllowRedirectOnSchemeChangeIsEnabled(HttpStatusCode statusCode)
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.org/foo"))
            {
                // Arrange
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");
                var redirectResponse = new HttpResponseMessage(statusCode);
                redirectResponse.Headers.Location = new Uri("http://example.org/bar");
                this._redirectHandler.RedirectOption.AllowRedirectOnSchemeChange = true;// Enable redirects on scheme change
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.NotSame(response.RequestMessage?.RequestUri?.Scheme, httpRequestMessage.RequestUri?.Scheme);
                Assert.Null(response.RequestMessage?.Headers.Authorization);
            }
        }

        [Fact]
        public async Task RedirectWithSameHostShouldKeepAuthHeader()
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo"))
            {
                // Arrange
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");
                var redirectResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
                redirectResponse.Headers.Location = new Uri("http://example.org/bar");
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.Equal(response.RequestMessage?.RequestUri?.Host, httpRequestMessage.RequestUri?.Host);
                Assert.NotNull(response.RequestMessage?.Headers.Authorization);
            }
        }

        [Fact]
        public async Task RedirectWithRelativeUrlShouldKeepRequestHost()
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo"))
            {   // Arrange
                var redirectResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
                redirectResponse.Headers.Location = new Uri("/bar", UriKind.Relative);
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.Equal("http://example.org/bar", response.RequestMessage?.RequestUri?.AbsoluteUri);
            }
        }

        [Fact]
        public async Task ExceedMaxRedirectsShouldThrowsException()
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo"))
            {
                // Arrange
                var response1 = new HttpResponseMessage(HttpStatusCode.Redirect);
                response1.Headers.Location = new Uri("http://example.org/bar");
                var response2 = new HttpResponseMessage(HttpStatusCode.Redirect);
                response2.Headers.Location = new Uri("http://example.org/foo");
                this._testHttpMessageHandler.SetHttpResponse(response1, response2);// sets the mock response
                // Act
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => this._invoker.SendAsync(
                   httpRequestMessage, CancellationToken.None));
                // Assert
                Assert.Equal("Too many redirects performed", exception.Message);
                Assert.Equal("Max redirects exceeded. Redirect count : 5", exception.InnerException?.Message);
                Assert.IsType<InvalidOperationException>(exception);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectWithDifferentHostShouldRemoveProxyAuthHeaderWhenNoProxyConfigured(HttpStatusCode statusCode)
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo"))
            {
                // Arrange - No proxy is configured, so ProxyAuthorization should be removed
                httpRequestMessage.Headers.ProxyAuthorization = new AuthenticationHeaderValue("fooAuth", "aparam");
                var redirectResponse = new HttpResponseMessage(statusCode);
                redirectResponse.Headers.Location = new Uri("http://example.net/bar");
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert - ProxyAuthorization is removed when no proxy is configured
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.NotSame(response.RequestMessage?.RequestUri?.Host, httpRequestMessage.RequestUri?.Host);
                Assert.Null(response.RequestMessage?.Headers.ProxyAuthorization);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectWithDifferentHostShouldRemoveCookieHeader(HttpStatusCode statusCode)
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo"))
            {
                // Arrange
                httpRequestMessage.Headers.Add("Cookie", "session=abc123");
                var redirectResponse = new HttpResponseMessage(statusCode);
                redirectResponse.Headers.Location = new Uri("http://example.net/bar");
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.NotSame(response.RequestMessage?.RequestUri?.Host, httpRequestMessage.RequestUri?.Host);
                Assert.False(response.RequestMessage?.Headers.Contains("Cookie"));
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectWithDifferentHostShouldRemoveCustomHeadersWithCustomCallback(HttpStatusCode statusCode)
        {
            // Arrange - Custom callback that removes additional headers on host change
            var redirectOption = new RedirectHandlerOption
            {
                ScrubSensitiveHeaders = (request, originalUri, newUri, proxyResolver) =>
                {
                    // Call default implementation first
                    RedirectHandlerOption.DefaultScrubSensitiveHeaders(request, originalUri, newUri, proxyResolver);

                    // Then add custom logic for additional headers
                    var isDifferentHostOrScheme = !newUri.Host.Equals(originalUri.Host, StringComparison.OrdinalIgnoreCase) ||
                        !newUri.Scheme.Equals(originalUri.Scheme, StringComparison.OrdinalIgnoreCase);
                    if(isDifferentHostOrScheme)
                    {
                        request.Headers.Remove("X-Api-Key");
                        request.Headers.Remove("X-Custom-Auth");
                    }
                }
            };
            var mockHandler = new MockRedirectHandler();
            var redirectHandler = new RedirectHandler(redirectOption)
            {
                InnerHandler = mockHandler
            };
            using var invoker = new HttpMessageInvoker(redirectHandler);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.Add("X-Api-Key", "secret-api-key");
            httpRequestMessage.Headers.Add("X-Custom-Auth", "custom-auth-value");
            httpRequestMessage.Headers.Add("X-Safe-Header", "should-remain");

            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.net/bar");
            mockHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - Custom headers should be removed, but non-sensitive should remain
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.False(response.RequestMessage?.Headers.Contains("X-Api-Key"));
            Assert.False(response.RequestMessage?.Headers.Contains("X-Custom-Auth"));
            Assert.True(response.RequestMessage?.Headers.Contains("X-Safe-Header"));
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectWithDifferentSchemeShouldRemoveCustomHeadersWithCustomCallback(HttpStatusCode statusCode)
        {
            // Arrange - Custom callback that removes additional headers on scheme change
            var redirectOption = new RedirectHandlerOption
            {
                AllowRedirectOnSchemeChange = true,
                ScrubSensitiveHeaders = (request, originalUri, newUri, proxyResolver) =>
                {
                    RedirectHandlerOption.DefaultScrubSensitiveHeaders(request, originalUri, newUri, proxyResolver);

                    var isDifferentHostOrScheme = !newUri.Host.Equals(originalUri.Host, StringComparison.OrdinalIgnoreCase) ||
                        !newUri.Scheme.Equals(originalUri.Scheme, StringComparison.OrdinalIgnoreCase);
                    if(isDifferentHostOrScheme)
                    {
                        request.Headers.Remove("X-Api-Key");
                    }
                }
            };
            var mockHandler = new MockRedirectHandler();
            var redirectHandler = new RedirectHandler(redirectOption)
            {
                InnerHandler = mockHandler
            };
            using var invoker = new HttpMessageInvoker(redirectHandler);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.org/foo");
            httpRequestMessage.Headers.Add("X-Api-Key", "secret-api-key");

            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");
            mockHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - Custom headers should be removed on scheme change
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.False(response.RequestMessage?.Headers.Contains("X-Api-Key"));
        }

        [Fact]
        public async Task RedirectWithSameHostAndSchemeCustomCallbackShouldKeepCustomHeaders()
        {
            // Arrange - Custom callback that only removes headers on host/scheme change
            var redirectOption = new RedirectHandlerOption
            {
                ScrubSensitiveHeaders = (request, originalUri, newUri, proxyResolver) =>
                {
                    RedirectHandlerOption.DefaultScrubSensitiveHeaders(request, originalUri, newUri, proxyResolver);

                    var isDifferentHostOrScheme = !newUri.Host.Equals(originalUri.Host, StringComparison.OrdinalIgnoreCase) ||
                        !newUri.Scheme.Equals(originalUri.Scheme, StringComparison.OrdinalIgnoreCase);
                    if(isDifferentHostOrScheme)
                    {
                        request.Headers.Remove("X-Api-Key");
                    }
                }
            };
            var mockHandler = new MockRedirectHandler();
            var redirectHandler = new RedirectHandler(redirectOption)
            {
                InnerHandler = mockHandler
            };
            using var invoker = new HttpMessageInvoker(redirectHandler);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.Add("X-Api-Key", "secret-api-key");

            var redirectResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar"); // Same host and scheme
            mockHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - Custom headers should be kept when host and scheme are the same
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.True(response.RequestMessage?.Headers.Contains("X-Api-Key"));
        }

        [Fact]
        public async Task DefaultScrubSensitiveHeadersMethodCanBeReferencedDirectly()
        {
            // Arrange - Verify that the default static method can be referenced and used
            var redirectOption = new RedirectHandlerOption
            {
                // Explicitly set to default to verify it's accessible
                ScrubSensitiveHeaders = RedirectHandlerOption.DefaultScrubSensitiveHeaders
            };
            var mockHandler = new MockRedirectHandler();
            var redirectHandler = new RedirectHandler(redirectOption)
            {
                InnerHandler = mockHandler
            };
            using var invoker = new HttpMessageInvoker(redirectHandler);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "token");

            var redirectResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
            redirectResponse.Headers.Location = new Uri("http://example.net/bar");
            mockHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - Default behavior should remove Authorization on host change
            Assert.Null(response.RequestMessage?.Headers.Authorization);
        }

#if !BROWSER
        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task CustomCallbackCanUseProxyResolverToKeepHeadersWhenProxyActive(HttpStatusCode statusCode)
        {
            // Arrange - Custom callback that only removes custom headers when proxy is inactive
            var mockProxy = new MockWebProxy(new Uri("http://proxy.example.com:8080")); // no bypasses
            var httpClientHandler = new HttpClientHandler { Proxy = mockProxy };
            var mockHandler = new MockDelegatingRedirectHandler
            {
                InnerHandler = httpClientHandler
            };
            var redirectOption = new RedirectHandlerOption
            {
                ScrubSensitiveHeaders = (request, originalUri, newUri, proxyResolver) =>
                {
                    // Call default implementation
                    RedirectHandlerOption.DefaultScrubSensitiveHeaders(request, originalUri, newUri, proxyResolver);

                    // Only remove X-Api-Key if proxy is inactive (like ProxyAuthorization logic)
                    var isProxyInactive = proxyResolver == null || proxyResolver(newUri) == null;
                    var isDifferentHostOrScheme = !newUri.Host.Equals(originalUri.Host, StringComparison.OrdinalIgnoreCase) ||
                        !newUri.Scheme.Equals(originalUri.Scheme, StringComparison.OrdinalIgnoreCase);
                    if(isProxyInactive && isDifferentHostOrScheme)
                    {
                        request.Headers.Remove("X-Api-Key");
                    }
                }
            };
            var redirectHandler = new RedirectHandler(redirectOption)
            {
                InnerHandler = mockHandler
            };

            using var invoker = new HttpMessageInvoker(redirectHandler);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.Add("X-Api-Key", "secret-api-key");

            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.net/bar"); // different host, but proxy is active
            mockHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - X-Api-Key should be kept because proxy is active
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.True(response.RequestMessage?.Headers.Contains("X-Api-Key"));
        }
#endif

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectWithDifferentSchemeShouldRemoveProxyAuthHeaderWhenNoProxyConfigured(HttpStatusCode statusCode)
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.org/foo"))
            {
                // Arrange - No proxy is configured, so ProxyAuthorization should be removed
                httpRequestMessage.Headers.ProxyAuthorization = new AuthenticationHeaderValue("fooAuth", "aparam");
                var redirectResponse = new HttpResponseMessage(statusCode);
                redirectResponse.Headers.Location = new Uri("http://example.org/bar");
                this._redirectHandler.RedirectOption.AllowRedirectOnSchemeChange = true;// Enable redirects on scheme change
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert - ProxyAuthorization is removed when no proxy is configured
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.NotSame(response.RequestMessage?.RequestUri?.Scheme, httpRequestMessage.RequestUri?.Scheme);
                Assert.Null(response.RequestMessage?.Headers.ProxyAuthorization);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectWithDifferentSchemeShouldRemoveCookieHeaderIfAllowRedirectOnSchemeChangeIsEnabled(HttpStatusCode statusCode)
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.org/foo"))
            {
                // Arrange
                httpRequestMessage.Headers.Add("Cookie", "session=abc123");
                var redirectResponse = new HttpResponseMessage(statusCode);
                redirectResponse.Headers.Location = new Uri("http://example.org/bar");
                this._redirectHandler.RedirectOption.AllowRedirectOnSchemeChange = true;// Enable redirects on scheme change
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.NotSame(response.RequestMessage?.RequestUri?.Scheme, httpRequestMessage.RequestUri?.Scheme);
                Assert.False(response.RequestMessage?.Headers.Contains("Cookie"));
            }
        }

        [Fact]
        public async Task RedirectWithSameHostShouldRemoveProxyAuthHeaderWhenNoProxyConfigured()
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo"))
            {
                // Arrange - No proxy is configured, so ProxyAuthorization should be removed
                httpRequestMessage.Headers.ProxyAuthorization = new AuthenticationHeaderValue("fooAuth", "aparam");
                var redirectResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
                redirectResponse.Headers.Location = new Uri("http://example.org/bar");
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert - ProxyAuthorization is removed when no proxy is configured
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.Equal(response.RequestMessage?.RequestUri?.Host, httpRequestMessage.RequestUri?.Host);
                Assert.Null(response.RequestMessage?.Headers.ProxyAuthorization);
            }
        }

        [Fact]
        public async Task RedirectWithSameHostShouldKeepCookieHeader()
        {
            using(var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo"))
            {
                // Arrange
                httpRequestMessage.Headers.Add("Cookie", "session=abc123");
                var redirectResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
                redirectResponse.Headers.Location = new Uri("http://example.org/bar");
                this._testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));// sets the mock response
                // Act
                var response = await _invoker.SendAsync(httpRequestMessage, new CancellationToken());
                // Assert
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.Equal(response.RequestMessage?.RequestUri?.Host, httpRequestMessage.RequestUri?.Host);
                Assert.True(response.RequestMessage?.Headers.Contains("Cookie"));
            }
        }

#if !BROWSER
        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectToBypassedProxyUrlShouldRemoveProxyAuthHeader(HttpStatusCode statusCode)
        {
            // Arrange - Create a handler chain with a proxy that bypasses "internal.local"
            var mockProxy = new MockWebProxy(new Uri("http://proxy.example.com:8080"), "internal.local");
            var httpClientHandler = new HttpClientHandler { Proxy = mockProxy };
            var mockHandler = new MockDelegatingRedirectHandler
            {
                InnerHandler = httpClientHandler
            };
            var redirectHandler = new RedirectHandler
            {
                InnerHandler = mockHandler
            };

            using var invoker = new HttpMessageInvoker(redirectHandler);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.ProxyAuthorization = new AuthenticationHeaderValue("Basic", "creds");

            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://internal.local/bar"); // Bypassed by proxy
            mockHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - ProxyAuthorization should be removed because internal.local is bypassed
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.Null(response.RequestMessage?.Headers.ProxyAuthorization);
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectToProxiedUrlShouldKeepProxyAuthHeader(HttpStatusCode statusCode)
        {
            // Arrange - Create a handler chain with a proxy that bypasses "internal.local"
            var mockProxy = new MockWebProxy(new Uri("http://proxy.example.com:8080"), "internal.local");
            var httpClientHandler = new HttpClientHandler { Proxy = mockProxy };
            var mockHandler = new MockDelegatingRedirectHandler
            {
                InnerHandler = httpClientHandler
            };
            var redirectHandler = new RedirectHandler
            {
                InnerHandler = mockHandler
            };

            using var invoker = new HttpMessageInvoker(redirectHandler);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.ProxyAuthorization = new AuthenticationHeaderValue("Basic", "creds");

            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar"); // NOT bypassed, requires proxy
            mockHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - ProxyAuthorization should be kept because example.org requires proxy
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage?.Headers.ProxyAuthorization);
        }

        [Fact]
        public async Task RedirectWithNoProxyConfiguredShouldRemoveProxyAuthHeader()
        {
            // Arrange - Create a handler chain without a proxy configured
            var httpClientHandler = new HttpClientHandler { Proxy = null };
            var mockHandler = new MockDelegatingRedirectHandler
            {
                InnerHandler = httpClientHandler
            };
            var redirectHandler = new RedirectHandler
            {
                InnerHandler = mockHandler
            };

            using var invoker = new HttpMessageInvoker(redirectHandler);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.ProxyAuthorization = new AuthenticationHeaderValue("Basic", "creds");

            var redirectResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");
            mockHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - ProxyAuthorization should be removed when no proxy is configured
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.Null(response.RequestMessage?.Headers.ProxyAuthorization);
        }
#endif

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task RedirectResponseWithoutLocationHeaderShouldThrow(HttpStatusCode statusCode)
        {
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            // Arrange - redirect response with no Location header set
            var redirectResponse = new HttpResponseMessage(statusCode); // Location intentionally omitted
            this._testHttpMessageHandler.SetHttpResponse(redirectResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => this._invoker.SendAsync(httpRequestMessage, CancellationToken.None));
            Assert.Contains("Unable to perform redirect as Location Header is not set in response", exception.Message);
            Assert.Contains(statusCode.ToString(), exception.InnerException?.Message);
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task ShouldNotRedirectWhenShouldRedirectDelegateReturnsFalse(HttpStatusCode statusCode)
        {
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            // Arrange - ShouldRedirect delegate always returns false
            this._redirectHandler.RedirectOption.ShouldRedirect = _ => false;
            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");
            this._testHttpMessageHandler.SetHttpResponse(redirectResponse);

            // Act
            var response = await this._invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - response is returned as-is without following the redirect
            Assert.Equal(statusCode, response.StatusCode);
            Assert.Same(response.RequestMessage, httpRequestMessage);
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        [InlineData((HttpStatusCode)308)] // 308
        public async Task ShouldNotRedirectWhenMaxRedirectIsZero(HttpStatusCode statusCode)
        {
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            // Arrange - MaxRedirect = 0 disables redirects entirely
            this._redirectHandler.RedirectOption.MaxRedirect = 0;
            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");
            this._testHttpMessageHandler.SetHttpResponse(redirectResponse);

            // Act
            var response = await this._invoker.SendAsync(httpRequestMessage, CancellationToken.None);

            // Assert - response is returned as-is without following the redirect
            Assert.Equal(statusCode, response.StatusCode);
            Assert.Same(response.RequestMessage, httpRequestMessage);
        }
    }
}
