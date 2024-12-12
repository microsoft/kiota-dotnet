using System.Net.Http;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware;

public class BodyInspectionHandlerTests : IDisposable
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
        var response = await invoker.SendAsync(request, default);

        // Then
        Assert.Equal("request test", GetStringFromStream(option.RequestBody!));
        Assert.Equal("request test", await request.Content.ReadAsStringAsync()); // response from option is separate from "normal" request stream
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
        await writer.FlushAsync();
        memoryStream.Seek(0, SeekOrigin.Begin);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost")
        {
            Content = new StreamContent(memoryStream)
        };
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/octet-stream"
        );

        var response = await invoker.SendAsync(request, default);

        // Then
        Assert.Equal("request test", GetStringFromStream(option.RequestBody!));
        Assert.Equal("request test", await request.Content.ReadAsStringAsync()); // response from option is separate from "normal" request stream
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsNullRequestBodyStreamWhenThereIsNoRequestBody()
    {
        var option = new BodyInspectionHandlerOption { InspectRequestBody = true, };
        using var invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
        var response = await invoker.SendAsync(request, default);

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
        var response = await invoker.SendAsync(request, default);

        // Then
        Assert.Equal("response test", GetStringFromStream(option.ResponseBody!));
        Assert.Equal("response test", await response.Content.ReadAsStringAsync()); // response from option is separate from "normal" response stream
    }

    [Fact]
    public async Task BodyInspectionHandlerGetsNullResponseBodyStreamWhenThereIsNoResponseBody()
    {
        var option = new BodyInspectionHandlerOption { InspectResponseBody = true, };
        using var invoker = GetMessageInvoker(new HttpResponseMessage(), option);

        // When
        var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
        var response = await invoker.SendAsync(request, default);

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
