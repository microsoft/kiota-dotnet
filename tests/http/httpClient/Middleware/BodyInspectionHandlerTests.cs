using System.Net.Http;
using System.Text.Json;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware;

public sealed class BodyInspectionHandlerTests : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    [Fact]
    public void BodyInspectionHandlerConstruction()
    {
        using var defaultValue = new BodyInspectionHandler();
        Assert.NotNull(defaultValue);
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsRequestBodyStream()
    {
        var option = new BodyInspectionHandlerOption { InspectRequestBody = true, };
        using var invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost")
        {
            Content = new StringContent("request test")
        };
        await invoker.SendAsync(request, TestContext.Current.CancellationToken);

        // Then
        Assert.Equal("request test", GetStringFromStream(option.RequestBody!));
        Assert.Equal("request test", await request.Content.ReadAsStringAsync(
#if NET5_0_OR_GREATER
            TestContext.Current.CancellationToken
#endif
        )); // response from option is separate from "normal" request stream
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsRequestBodyStreamWhenRequestIsOctetStream()
    {
        var option = new BodyInspectionHandlerOption { InspectRequestBody = true, };
        using var invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream);
        await writer.WriteAsync("request test");
        await writer.FlushAsync(
#if NET5_0_OR_GREATER
            TestContext.Current.CancellationToken
#endif
        );
        memoryStream.Seek(0, SeekOrigin.Begin);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost")
        {
            Content = new StreamContent(memoryStream)
        };
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/octet-stream"
        );

        await invoker.SendAsync(request, TestContext.Current.CancellationToken);

        // Then
        Assert.Equal("request test", GetStringFromStream(option.RequestBody!));
        Assert.Equal("request test", await request.Content.ReadAsStringAsync(
#if NET5_0_OR_GREATER
            TestContext.Current.CancellationToken
#endif
        )); // response from option is separate from "normal" request stream
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsNullRequestBodyStreamWhenThereIsNoRequestBody()
    {
        var option = new BodyInspectionHandlerOption { InspectRequestBody = true, };
        using var invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
        await invoker.SendAsync(request, TestContext.Current.CancellationToken);

        // Then
        Assert.Same(Stream.Null, option.RequestBody);
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsResponseBodyStream()
    {
        var option = new BodyInspectionHandlerOption { InspectResponseBody = true, };
        using var invoker = GetMessageInvoker(CreateHttpResponseWithBody(), option);

        // When
        var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
        var response = await invoker.SendAsync(request, TestContext.Current.CancellationToken);

        // Then
        Assert.Equal("response test", GetStringFromStream(option.ResponseBody!));
        Assert.Equal("response test", await response.Content.ReadAsStringAsync(
#if NET5_0_OR_GREATER
            TestContext.Current.CancellationToken
#endif
        )); // response from option is separate from "normal" response stream
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsResponseBodyStreamFromKiotaClientFactory()
    {
        var option = new BodyInspectionHandlerOption { InspectResponseBody = true, InspectRequestBody = true };
        var messageHandler = new MockRedirectHandler();
        _disposables.Add(messageHandler);
        var responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("""{"owner":{"login":"microsoft"}}""")
        };
        _disposables.Add(responseMessage);
        messageHandler.SetHttpResponse(responseMessage);
        var httpClient = KiotaClientFactory.Create(messageHandler, [option]);
        _disposables.Add(httpClient);

        // When
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/repos/microsoft/kiota-dotnet");
        var response = await httpClient.SendAsync(request, TestContext.Current.CancellationToken);

        // Then
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotEqual(Stream.Null, option.ResponseBody);
        var jsonFromInspection = await JsonDocument.ParseAsync(option.ResponseBody, cancellationToken: TestContext.Current.CancellationToken);
        var jsonFromContent = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(
#if NET5_0_OR_GREATER
                TestContext.Current.CancellationToken
#endif
            ), cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(jsonFromInspection.RootElement.TryGetProperty("owner", out _));
        Assert.True(jsonFromContent.RootElement.TryGetProperty("owner", out _));
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsNullResponseBodyStreamWhenThereIsNoResponseBody()
    {
        var option = new BodyInspectionHandlerOption { InspectResponseBody = true, };
        using var invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
        await invoker.SendAsync(request, TestContext.Current.CancellationToken);

        // Then
        Assert.Same(Stream.Null, option.ResponseBody);
    }

    private static HttpResponseMessage CreateHttpResponseWithBody() =>
        new() { Content = new StringContent("response test") };

    private HttpMessageInvoker GetMessageInvoker(
        HttpResponseMessage httpResponseMessage,
        BodyInspectionHandlerOption option
    )
    {
        var messageHandler = new MockRedirectHandler();
        _disposables.Add(messageHandler);
        _disposables.Add(httpResponseMessage);
        messageHandler.SetHttpResponse(httpResponseMessage);
        // Given
        var handler = new BodyInspectionHandler(option) { InnerHandler = messageHandler };
        _disposables.Add(handler);
        return new HttpMessageInvoker(handler);
    }

    private static string GetStringFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void Dispose()
    {
        _disposables.ForEach(static x => x.Dispose());
        GC.SuppressFinalize(this);
    }
}
