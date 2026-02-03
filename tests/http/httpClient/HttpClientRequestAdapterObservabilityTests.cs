using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests
{
    public class HttpClientRequestAdapterObservabilityTests : IDisposable
    {
        private readonly HttpClientRequestAdapter _requestAdapter;
        private readonly ActivityListener _activityListener;
        private readonly List<Activity> _capturedActivities;

        public HttpClientRequestAdapterObservabilityTests()
        {
            var authProvider = new AnonymousAuthenticationProvider();
            var observabilityOptions = new ObservabilityOptions();
            _requestAdapter = new HttpClientRequestAdapter(authProvider, observabilityOptions: observabilityOptions);
            _requestAdapter.BaseUrl = "https://graph.microsoft.com/v1.0";

            // Setup activity listener to capture activities
            _capturedActivities = new List<Activity>();
            _activityListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == observabilityOptions.TracerInstrumentationName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = activity => _capturedActivities.Add(activity)
            };
            ActivitySource.AddActivityListener(_activityListener);
        }

        public void Dispose()
        {
            _activityListener?.Dispose();
            _requestAdapter?.Dispose();
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

        #endregion
    }
}
