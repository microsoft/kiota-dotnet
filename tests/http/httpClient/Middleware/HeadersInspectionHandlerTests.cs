using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware;

public sealed class HeadersInspectionHandlerTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    [Fact]
    public void HeadersInspectionHandlerConstruction()
    {
        using var defaultValue = new HeadersInspectionHandler();
        Assert.NotNull(defaultValue);
    }

    [Fact]
    public async Task HeadersInspectionHandlerGetsRequestHeaders()
    {
        var option = new HeadersInspectionHandlerOption
        {
            InspectRequestHeaders = true,
        };
        using var invoker = GetMessageInvoker(option);

        // When
        var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
        request.Headers.Add("test", "test");
        await invoker.SendAsync(request, TestContext.Current.CancellationToken);

        // Then
        Assert.Equal("test", option.RequestHeaders["test"].First());
        Assert.Empty(option.ResponseHeaders);
    }
    [Fact]
    public async Task HeadersInspectionHandlerGetsResponseHeaders()
    {
        var option = new HeadersInspectionHandlerOption
        {
            InspectResponseHeaders = true,
        };
        using var invoker = GetMessageInvoker(option);

        // When
        var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
        await invoker.SendAsync(request, TestContext.Current.CancellationToken);

        // Then
        Assert.Equal("test", option.ResponseHeaders["test"].First());
        Assert.Empty(option.RequestHeaders);
    }
    private HttpMessageInvoker GetMessageInvoker(HeadersInspectionHandlerOption? option = null)
    {
        var messageHandler = new MockRedirectHandler();
        _disposables.Add(messageHandler);
        var response = new HttpResponseMessage();
        response.Headers.Add("test", "test");
        _disposables.Add(response);
        messageHandler.SetHttpResponse(response);
        // Given
        var handler = new HeadersInspectionHandler(option)
        {
            InnerHandler = messageHandler
        };
        _disposables.Add(handler);
        return new HttpMessageInvoker(handler);
    }

    public void Dispose()
    {
        _disposables.ForEach(static x => x.Dispose());
        GC.SuppressFinalize(this);
    }
}
