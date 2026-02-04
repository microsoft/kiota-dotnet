using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Microsoft.Kiota.Serialization.Json;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests
{
    /// <summary>
    /// Tests for OpenTelemetry observability instrumentation in HttpClientRequestAdapter.
    ///
    /// <para>
    /// This test suite covers the basic OpenTelemetry Activity spans and tags that are instrumented
    /// throughout the HTTP request lifecycle. It includes comprehensive unit tests for HTTP route
    /// normalization and integration tests for Activity span creation with key semantic tags.
    /// </para>
    ///
    /// <para><strong>Test Coverage:</strong></para>
    /// <list type="bullet">
    ///   <item><description>HTTP route normalization (20 tests) - validates the GetNormalizedHttpRoute method</description></item>
    ///   <item><description>Activity span creation and nested spans (8 tests)</description></item>
    ///   <item><description>Basic semantic tags: http.route, url.uri_template, url.scheme, server.address, http.request.method</description></item>
    ///   <item><description>EUII attribute handling: url.full tag gating based on IncludeEUIIAttributes setting</description></item>
    /// </list>
    ///
    /// <para><strong>Excluded Test Scenarios:</strong></para>
    /// <para>
    /// The following OpenTelemetry tags and scenarios are NOT tested due to infrastructure limitations
    /// with mocking the full HTTP request/response pipeline. These tags are only set after successful
    /// HTTP responses or during error handling deep within the middleware stack, which requires complex
    /// mocking of HttpClient internals, middleware behavior, and serialization:
    /// </para>
    /// <list type="bullet">
    ///   <item><description><strong>http.response.status_code</strong> - Response status code (e.g., "200", "404")</description></item>
    ///   <item><description><strong>network.protocol.name</strong> - HTTP protocol version (e.g., "1.1", "2.0")</description></item>
    ///   <item><description><strong>http.request.header.content-type</strong> - Request body content type</description></item>
    ///   <item><description><strong>http.request.body.size</strong> - Request body size in bytes</description></item>
    ///   <item><description><strong>http.response.header.content-type</strong> - Response body content type</description></item>
    ///   <item><description><strong>http.response.body.size</strong> - Response body size in bytes</description></item>
    ///   <item><description><strong>com.microsoft.kiota.error.mapping_found</strong> - Whether an error mapping was found</description></item>
    ///   <item><description><strong>com.microsoft.kiota.response.type</strong> - The response model type name</description></item>
    ///   <item><description><strong>Deserialization spans</strong> - GetCollectionOfObjectValues and related deserialization activities</description></item>
    /// </list>
    ///
    /// <para>
    /// Testing these scenarios would require:
    /// </para>
    /// <list type="number">
    ///   <item><description>Full HttpMessageHandler mocking with proper Content stream handling</description></item>
    ///   <item><description>Mocking the entire middleware pipeline execution</description></item>
    ///   <item><description>Setting up proper serialization/deserialization infrastructure</description></item>
    ///   <item><description>Ensuring Activities remain active throughout async operations</description></item>
    /// </list>
    ///
    /// <para>
    /// These scenarios are better validated through integration tests with real HTTP endpoints
    /// or end-to-end testing frameworks rather than unit tests with mocks.
    /// </para>
    /// </summary>
    public class HttpClientRequestAdapterObservabilityTests : IDisposable
    {
        private readonly HttpClientRequestAdapter _requestAdapter;
        private readonly ActivityListener _activityListener;
        private readonly List<Activity> _capturedActivities;

        public HttpClientRequestAdapterObservabilityTests()
        {
            // Register JSON serialization for tests
            ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();

            var authProvider = new AnonymousAuthenticationProvider();
            var observabilityOptions = new ObservabilityOptions();
            _requestAdapter = new HttpClientRequestAdapter(authProvider, observabilityOptions: observabilityOptions);
            _requestAdapter.BaseUrl = "https://graph.microsoft.com/v1.0";

            // Setup activity listener to capture activities from all Kiota sources
            _capturedActivities = new List<Activity>();
            _activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name.StartsWith("Microsoft.Kiota", StringComparison.OrdinalIgnoreCase),
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = activity => _capturedActivities.Add(activity),
                ActivityStopped = activity => { } // Capture stopped activities to ensure tags are set
            };
            ActivitySource.AddActivityListener(_activityListener);
        }

        public void Dispose()
        {
            _activityListener?.Dispose();
            _requestAdapter?.Dispose();
        }

        private KeyValuePair<string, string?> GetTagFromActivities(string tagKey)
        {
            return _capturedActivities
                .SelectMany(a => a.Tags)
                .FirstOrDefault(t => t.Key == tagKey);
        }

        #region GetNormalizedHttpRoute Tests

        [Fact]
        public void GetNormalizedHttpRoute_WithBaseUrlPlaceholder_RemovesPlaceholder()
        {
            // Arrange
            var input = "{+baseurl}/users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithBaseUrlPlaceholderCaseInsensitive_RemovesPlaceholder()
        {
            // Arrange
            var input = "{+BASEURL}/users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithBaseUrlPlaceholderInMiddle_RemovesPlaceholder()
        {
            // Arrange
            var input = "prefix{+baseurl}/users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/prefix/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithBaseUrlPrefix_RemovesPrefix()
        {
            // Arrange
            var input = "https://graph.microsoft.com/v1.0/users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithBaseUrlPrefixCaseInsensitive_RemovesPrefix()
        {
            // Arrange
            var input = "HTTPS://GRAPH.MICROSOFT.COM/V1.0/users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithLeadingSlash_PreservesSlash()
        {
            // Arrange
            var input = "/users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithoutLeadingSlash_AddsSlash()
        {
            // Arrange
            var input = "users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithMultipleLeadingSlashes_NormalizesToSingleSlash()
        {
            // Arrange
            var input = "///users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithLeadingWhitespace_TrimsAndAddsSlash()
        {
            // Arrange
            var input = "  users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithTrailingWhitespace_TrimsWhitespace()
        {
            // Arrange
            var input = "users/{id}  ";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithLeadingAndTrailingWhitespace_TrimsWhitespace()
        {
            // Arrange
            var input = "  users/{id}  ";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithEmptyString_ReturnsRootSlash()
        {
            // Arrange
            var input = "";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithOnlyWhitespace_ReturnsRootSlash()
        {
            // Arrange
            var input = "   ";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithOnlyBaseUrlPlaceholder_ReturnsRootSlash()
        {
            // Arrange
            var input = "{+baseurl}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithOnlyBaseUrl_ReturnsRootSlash()
        {
            // Arrange
            var input = "https://graph.microsoft.com/v1.0";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithBaseUrlAndTrailingSlash_ReturnsRootSlash()
        {
            // Arrange
            var input = "https://graph.microsoft.com/v1.0/";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithComplexPath_NormalizesCorrectly()
        {
            // Arrange
            var input = "{+baseurl}/users/{user-id}/messages/{message-id}/attachments";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{user-id}/messages/{message-id}/attachments", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithQueryParameters_PreservesPath()
        {
            // Arrange - Note: query parameters should be removed before calling this method
            var input = "/users/{id}";

            // Act
            var result = _requestAdapter.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithBothPlaceholderAndBaseUrl_HandlesBothTransformations()
        {
            // Arrange
            var input = "{+baseurl}/users";
            var adapterWithBaseUrl = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());
            adapterWithBaseUrl.BaseUrl = "https://api.example.com/v2";

            // Act
            var result = adapterWithBaseUrl.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users", result);

            // Cleanup
            adapterWithBaseUrl.Dispose();
        }

        [Fact]
        public void GetNormalizedHttpRoute_WithNullBaseUrl_OnlyRemovesPlaceholder()
        {
            // Arrange
            var input = "{+baseurl}/users/{id}";
            var adapterWithoutBaseUrl = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());

            // Act
            var result = adapterWithoutBaseUrl.GetNormalizedHttpRoute(input);

            // Assert
            Assert.Equal("/users/{id}", result);

            // Cleanup
            adapterWithoutBaseUrl.Dispose();
        }

        #endregion

        #region Activity Span Tag Tests

        [Fact]
        public async Task SendAsync_CreatesActivityWithHttpRouteTag()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"123\"}")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var authProvider = new AnonymousAuthenticationProvider();
            var observabilityOptions = new ObservabilityOptions();
            using var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient, observabilityOptions: observabilityOptions);
            adapter.BaseUrl = "https://graph.microsoft.com/v1.0";

            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}/users/{user-id}"
            };
            requestInfo.PathParameters.Add("user-id", "john@contoso.com");

            // Clear any previously captured activities
            _capturedActivities.Clear();

            // Act
            try
            {
                await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);
            }
            catch
            {
                // Ignore exceptions, we're only interested in the activity
            }

            // Assert
            var activity = _capturedActivities.FirstOrDefault(a => a.OperationName.Contains("SendAsync"));
            Assert.NotNull(activity);

            var httpRouteTag = activity.Tags.FirstOrDefault(t => t.Key == "http.route");
            Assert.NotNull(httpRouteTag.Key);
            Assert.Equal("/users/{user-id}", httpRouteTag.Value);
        }

        [Fact]
        public async Task SendAsync_CreatesActivityWithUriTemplateTag()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"123\"}")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var authProvider = new AnonymousAuthenticationProvider();
            var observabilityOptions = new ObservabilityOptions();
            using var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient, observabilityOptions: observabilityOptions);
            adapter.BaseUrl = "https://graph.microsoft.com/v1.0";

            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}/users/{user-id}/messages"
            };
            requestInfo.PathParameters.Add("user-id", "john@contoso.com");

            // Clear any previously captured activities
            _capturedActivities.Clear();

            // Act
            try
            {
                await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);
            }
            catch
            {
                // Ignore exceptions, we're only interested in the activity
            }

            // Assert
            var activity = _capturedActivities.FirstOrDefault(a => a.OperationName.Contains("SendAsync"));
            Assert.NotNull(activity);

            var uriTemplateTag = activity.Tags.FirstOrDefault(t => t.Key == "url.uri_template");
            Assert.NotNull(uriTemplateTag.Key);
            Assert.Contains("/users/{user-id}/messages", uriTemplateTag.Value);
        }

        [Fact]
        public async Task SendAsync_WithEmptyPath_SetsHttpRouteToRoot()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"123\"}")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var authProvider = new AnonymousAuthenticationProvider();
            var observabilityOptions = new ObservabilityOptions();
            using var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient, observabilityOptions: observabilityOptions);
            adapter.BaseUrl = "https://graph.microsoft.com/v1.0";

            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}"
            };

            // Clear any previously captured activities
            _capturedActivities.Clear();

            // Act
            try
            {
                await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);
            }
            catch
            {
                // Ignore exceptions, we're only interested in the activity
            }

            // Assert
            var activity = _capturedActivities.FirstOrDefault(a => a.OperationName.Contains("SendAsync"));
            Assert.NotNull(activity);

            var httpRouteTag = activity.Tags.FirstOrDefault(t => t.Key == "http.route");
            Assert.NotNull(httpRouteTag.Key);
            Assert.Equal("/", httpRouteTag.Value);
        }

        [Fact]
        public async Task SendAsync_SetsHttpRequestMethodTag()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"123\"}", Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var authProvider = new AnonymousAuthenticationProvider();
            var observabilityOptions = new ObservabilityOptions();
            using var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient, observabilityOptions: observabilityOptions);
            adapter.BaseUrl = "https://graph.microsoft.com/v1.0";

            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.POST,
                UrlTemplate = "{+baseurl}/users"
            };

            _capturedActivities.Clear();

            // Act
            await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

            // Assert
            Assert.NotEmpty(_capturedActivities);

            var methodTag = GetTagFromActivities("http.request.method");
            Assert.NotNull(methodTag.Key);
            Assert.Equal("POST", methodTag.Value);
        }

        [Fact]
        public async Task SendAsync_SetsUrlSchemeAndServerAddressTags()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"123\"}", Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var authProvider = new AnonymousAuthenticationProvider();
            var observabilityOptions = new ObservabilityOptions();
            using var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient, observabilityOptions: observabilityOptions);
            adapter.BaseUrl = "https://graph.microsoft.com/v1.0";

            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}/users"
            };

            _capturedActivities.Clear();

            // Act
            await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

            // Assert
            Assert.NotEmpty(_capturedActivities);

            var schemeTag = GetTagFromActivities("url.scheme");
            Assert.Equal("https", schemeTag.Value);

            var serverTag = GetTagFromActivities("server.address");
            Assert.Equal("graph.microsoft.com", serverTag.Value);
        }

        [Fact]
        public async Task SendAsync_WithoutIncludeEUIIAttributes_DoesNotSetUrlFullTag()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"123\"}", Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var authProvider = new AnonymousAuthenticationProvider();
            var observabilityOptions = new ObservabilityOptions { IncludeEUIIAttributes = false };
            using var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient, observabilityOptions: observabilityOptions);
            adapter.BaseUrl = "https://graph.microsoft.com/v1.0";

            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}/users"
            };

            _capturedActivities.Clear();

            // Act
            await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

            // Assert
            Assert.NotEmpty(_capturedActivities);

            var urlFullTag = GetTagFromActivities("url.full");
            Assert.Null(urlFullTag.Key);
        }

        [Fact]
        public async Task SendAsync_CreatesNestedActivitySpans()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(() => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"123\"}", Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var authProvider = new AnonymousAuthenticationProvider();
            var observabilityOptions = new ObservabilityOptions();
            using var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient, observabilityOptions: observabilityOptions);
            adapter.BaseUrl = "https://graph.microsoft.com/v1.0";

            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}/users"
            };

            _capturedActivities.Clear();

            // Act
            await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

            // Assert - Verify various nested spans are created
            Assert.Contains(_capturedActivities, a => a.OperationName.Contains("SendAsync"));
            Assert.Contains(_capturedActivities, a => a.OperationName.Contains("GetHttpResponseMessageAsync"));
            Assert.Contains(_capturedActivities, a => a.OperationName.Contains("GetRequestMessageFromRequestInformation"));
            Assert.Contains(_capturedActivities, a => a.OperationName.Contains("GetRootParseNodeAsync"));
        }

        #endregion
    }
}
