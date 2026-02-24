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

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests;

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
public sealed class HttpClientRequestAdapterObservabilityTests : IDisposable
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

    private KeyValuePair<string, string?> GetTagFromActivities(string tagKey, ActivityTraceId traceId)
    {
        return _capturedActivities
            .Where(a => a.TraceId == traceId)
            .SelectMany(a => a.Tags)
            .FirstOrDefault(t => t.Key == tagKey);
    }

    private static Activity StartTestRootActivity()
    {
        var activity = new Activity("test");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();
        return activity;
    }

    #region GetNormalizedHttpRoute Tests

    [Theory]
    [InlineData("{+baseurl}/users/{id}", "/users/{id}")]
    [InlineData("{+BASEURL}/users/{id}", "/users/{id}")]
    [InlineData("prefix{+baseurl}/users/{id}", "/prefix/users/{id}")]
    [InlineData("https://graph.microsoft.com/v1.0/users/{id}", "/users/{id}")]
    [InlineData("HTTPS://GRAPH.MICROSOFT.COM/V1.0/users/{id}", "/users/{id}")]
    [InlineData("/users/{id}", "/users/{id}")]
    [InlineData("users/{id}", "/users/{id}")]
    [InlineData("///users/{id}", "/users/{id}")]
    [InlineData("  users/{id}", "/users/{id}")]
    [InlineData("users/{id}  ", "/users/{id}")]
    [InlineData("  users/{id}  ", "/users/{id}")]
    [InlineData("", "/")]
    [InlineData("   ", "/")]
    [InlineData("{+baseurl}", "/")]
    [InlineData("https://graph.microsoft.com/v1.0", "/")]
    [InlineData("https://graph.microsoft.com/v1.0/", "/")]
    [InlineData("{+baseurl}/users/{user-id}/messages/{message-id}/attachments", "/users/{user-id}/messages/{message-id}/attachments")]
    public void GetNormalizedHttpRoute_NormalizesCorrectly(string input, string expected)
    {
        // Act
        var result = _requestAdapter.GetNormalizedHttpRoute(input);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Activity Span Tag Tests

    [Theory]
    [InlineData("{+baseurl}/users/{user%2Did}", "user%2Did", "john@contoso.com", "/users/{user-id}")]
    [InlineData("{+baseurl}", null, null, "/")]
    [InlineData("{+baseurl}/users/{id}{?%24select,%24expand}", "id", "123", "/users/{id}")]
    [InlineData("{+baseurl}/users{?%24filter}", null, null, "/users")]
    [InlineData("{+baseurl}/users/{user%2Did}/messages{?%24top,%24skip}", "user%2Did", "alice@contoso.com", "/users/{user-id}/messages")]
    [InlineData("{+baseurl}/me/drive/items/{item%2Did}{?%24select}", "item%2Did", "item123", "/me/drive/items/{item-id}")]
    [InlineData("{+baseurl}/groups/{group%2Did}/members{?%24count}", "group%2Did", "group456", "/groups/{group-id}/members")]
    [InlineData("{+baseurl}/groups/{group%2Did}/members/$ref?@id={%40id}", "group%2Did", "group456", "/groups/{group-id}/members/$ref")]
    [InlineData("{+baseurl}/groups/{group%2Did}/members/$ref{?%24count,%24filter}", "group%2Did", "group456", "/groups/{group-id}/members/$ref")]
    [InlineData("{+baseurl}/users/{id}{?%24filter,q}", "id", "789", "/users/{id}")]
    [InlineData("{+baseurl}/drive/items/{item%2Did}/workbook/worksheets/{worksheet%2Did}/range(address='{address}'){?%24select,%24expand,%24top}", "item%2Did", "book123", "/drive/items/{item-id}/workbook/worksheets/{worksheet-id}/range(address='{address}')")]
    public async Task SendAsync_CreatesActivityWithHttpRouteTag(string urlTemplate, string? pathParamKey, string? pathParamValue, string expectedRoute)
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

        var pathParameters = new Dictionary<string, object>();
        if(pathParamKey is not null && pathParamValue is not null)
        {
            pathParameters.Add(pathParamKey, pathParamValue);
        }
        var requestInfo = new RequestInformation(Method.GET, urlTemplate, pathParameters);
        // Clear any previously captured activities
        _capturedActivities.Clear();
        using var testRoot = StartTestRootActivity();

        // Act
        await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

        // Assert
        var activity = _capturedActivities.FirstOrDefault(a => a.TraceId == testRoot.TraceId && a.OperationName.Contains("SendAsync"));
        Assert.NotNull(activity);

        var httpRouteTag = activity.Tags.FirstOrDefault(t => t.Key == "http.route");
        Assert.NotNull(httpRouteTag.Key);
        Assert.Equal(expectedRoute, httpRouteTag.Value);
    }

    [Theory]
    [InlineData("{+baseurl}/users/{user%2Did}", "user%2Did", "john@contoso.com", "/users/{user-id}")]
    [InlineData("{+baseurl}", null, null, "{+baseurl}")]
    [InlineData("{+baseurl}/users/{id}{?%24select,%24expand}", "id", "123", "/users/{id}{?$select,$expand}")]
    [InlineData("{+baseurl}/users{?%24filter}", null, null, "/users{?$filter}")]
    [InlineData("{+baseurl}/users/{user%2Did}/messages{?%24top,%24skip}", "user%2Did", "alice@contoso.com", "/users/{user-id}/messages{?$top,$skip}")]
    [InlineData("{+baseurl}/me/drive/items/{item%2Did}{?%24select}", "item%2Did", "item123", "/me/drive/items/{item-id}{?$select}")]
    [InlineData("{+baseurl}/groups/{group%2Did}/members{?%24count}", "group%2Did", "group456", "/groups/{group-id}/members{?$count}")]
    [InlineData("{+baseurl}/groups/{group%2Did}/members/$ref?@id={%40id}", "group%2Did", "group456", "members/$ref")]
    [InlineData("{+baseurl}/groups/{group%2Did}/members/$ref{?%24count,%24filter}", "group%2Did", "group456", "/groups/{group-id}/members/$ref{?$count,$filter}")]
    [InlineData("{+baseurl}/users/{id}{?%24filter,q}", "id", "789", "/users/{id}{?$filter,q}")]
    [InlineData("{+baseurl}/drive/items/{item%2Did}/workbook/worksheets/{worksheet%2Did}/range(address='{address}'){?%24select,%24expand,%24top}", "item%2Did", "book123", "workbook/worksheets")]
    public async Task SendAsync_CreatesActivityWithUriTemplateTag(string urlTemplate, string? pathParamKey, string? pathParamValue, string expectedSubstring)
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

        var pathParameters = new Dictionary<string, object>();
        if(pathParamKey is not null && pathParamValue is not null)
        {
            pathParameters.Add(pathParamKey, pathParamValue);
        }
        var requestInfo = new RequestInformation(Method.GET, urlTemplate, pathParameters);

        // Clear any previously captured activities
        _capturedActivities.Clear();
        using var testRoot = StartTestRootActivity();

        // Act
        await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

        // Assert
        var activity = _capturedActivities.FirstOrDefault(a => a.TraceId == testRoot.TraceId && a.OperationName.Contains("SendAsync"));
        Assert.NotNull(activity);

        var uriTemplateTag = activity.Tags.FirstOrDefault(t => t.Key == "url.uri_template");
        Assert.NotNull(uriTemplateTag.Key);
        Assert.Contains(expectedSubstring, uriTemplateTag.Value);
    }

    [Theory]
    [InlineData(Method.GET, "GET")]
    [InlineData(Method.POST, "POST")]
    [InlineData(Method.PUT, "PUT")]
    [InlineData(Method.PATCH, "PATCH")]
    [InlineData(Method.DELETE, "DELETE")]
    [InlineData(Method.HEAD, "HEAD")]
    [InlineData(Method.OPTIONS, "OPTIONS")]
    public async Task SendAsync_SetsHttpRequestMethodTag(Method httpMethod, string expectedMethodTag)
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
            HttpMethod = httpMethod,
            UrlTemplate = "{+baseurl}/users"
        };

        _capturedActivities.Clear();
        using var testRoot = StartTestRootActivity();

        // Act
        await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

        // Assert
        var testActivities = _capturedActivities.Where(a => a.TraceId == testRoot.TraceId).ToList();
        Assert.NotEmpty(testActivities);

        var methodTag = GetTagFromActivities("http.request.method", testRoot.TraceId);
        Assert.NotNull(methodTag.Key);
        Assert.Equal(expectedMethodTag, methodTag.Value);
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
        using var testRoot = StartTestRootActivity();

        // Act
        await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

        // Assert
        var testActivities = _capturedActivities.Where(a => a.TraceId == testRoot.TraceId).ToList();
        Assert.NotEmpty(testActivities);

        var schemeTag = GetTagFromActivities("url.scheme", testRoot.TraceId);
        Assert.Equal("https", schemeTag.Value);

        var serverTag = GetTagFromActivities("server.address", testRoot.TraceId);
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
        using var testRoot = StartTestRootActivity();

        // Act
        await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

        // Assert
        var testActivities = _capturedActivities.Where(a => a.TraceId == testRoot.TraceId).ToList();
        Assert.NotEmpty(testActivities);

        var urlFullTag = GetTagFromActivities("url.full", testRoot.TraceId);
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
        using var testRoot = StartTestRootActivity();

        // Act
        await adapter.SendAsync<MockEntity>(requestInfo, MockEntity.Factory);

        // Assert - Verify various nested spans are created
        var resultingActivities = new HashSet<string>(
            _capturedActivities.ToList().Where(a => a.TraceId == testRoot.TraceId).Select(static a => a.OperationName),
            StringComparer.Ordinal);
        Assert.Contains("SendAsync - {+baseurl}/users", resultingActivities);
        Assert.Contains("GetHttpResponseMessageAsync", resultingActivities);
        Assert.Contains("GetRequestMessageFromRequestInformation", resultingActivities);
        Assert.Contains("GetRootParseNodeAsync", resultingActivities);
    }

    #endregion
}
