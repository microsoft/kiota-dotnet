using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware;

public class UriReplacementOptionTests {
    [Fact]
    public void Does_Nothing_When_Url_Replacement_Is_Disabled()
    {
        var uri = new Uri("http://localhost/test");
        var disabled = new UriReplacementHandlerOption(false, new Dictionary<string, string>());

        Assert.False(disabled.IsEnabled());
        Assert.Equal(uri, disabled.Replace(uri));

        disabled = new UriReplacementHandlerOption(false, new Dictionary<string, string>{
            {"test", ""}
        });

        Assert.Equal(uri, disabled.Replace(uri));
    }

    [Fact]
    public void Returns_Null_When_Url_Provided_Is_Null()
    {
        var disabled = new UriReplacementHandlerOption(false, new Dictionary<string, string>());

        Assert.False(disabled.IsEnabled());
        Assert.Null(disabled.Replace(null));
    }

    [Fact]
    public void Replaces_Key_In_Path_With_Value()
    {
        var uri = new Uri("http://localhost/test");
        var option = new UriReplacementHandlerOption(true, new Dictionary<string, string>{{"test", ""}});

        Assert.True(option.IsEnabled());
        Assert.Equal("http://localhost/", option.Replace(uri)!.ToString());
    }
}

public class UriReplacementHandlerTests
{
    [Fact]
    public async Task Calls_Uri_ReplacementAsync()
    {
        var mockReplacement = new Mock<IUriReplacementHandlerOption>();
        mockReplacement.Setup(static x => x.IsEnabled()).Returns(true);
        mockReplacement.Setup(static x => x.Replace(It.IsAny<Uri>())).Returns(new Uri("http://changed"));

        var handler = new UriReplacementHandler<IUriReplacementHandlerOption>(mockReplacement.Object)
        {
            InnerHandler = new FakeSuccessHandler()
        };
        var msg = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
        var client = new HttpClient(handler);
        await client.SendAsync(msg);

        mockReplacement.Verify(static x=> x.Replace(It.IsAny<Uri>()), Times.Once());
    }

    [Fact]
    public async Task Calls_Uri_Replacement_From_Request_OptionsAsync()
    {
        var mockReplacement = new Mock<IUriReplacementHandlerOption>();
        mockReplacement.Setup(static x => x.IsEnabled()).Returns(true);
        mockReplacement.Setup(static x => x.Replace(It.IsAny<Uri>())).Returns(new Uri("http://changed"));

        var handler = new UriReplacementHandler<IUriReplacementHandlerOption>()
        {
            InnerHandler = new FakeSuccessHandler()
        };
        var msg = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
        SetRequestOption(msg, mockReplacement.Object);
        var client = new HttpClient(handler);
        await client.SendAsync(msg);

        mockReplacement.Verify(static x=> x.Replace(It.IsAny<Uri>()), Times.Once());
    }

    /// <summary>
    /// Sets a <see cref="IRequestOption"/> in <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> representation of the request.</param>
    /// <param name="option">The request option.</param>
    private static void SetRequestOption<T>(HttpRequestMessage httpRequestMessage, T option) where T : IRequestOption
    {
#if NET5_0_OR_GREATER
        httpRequestMessage.Options.Set(new HttpRequestOptionsKey<T>(typeof(T).FullName!), option);
#else
        httpRequestMessage.Properties.Add(typeof(T).FullName!, option);
#endif
    }
}
