using System.Text;

namespace Microsoft.Kiota.Serialization.Form.Tests;

public class FormAsyncParseNodeFactoryTests
{
    private readonly FormParseNodeFactory _formParseNodeFactory = new();
    private const string TestJsonString = "key=value";
    [Fact]
    public async Task GetsWriterForFormContentType()
    {
        using var formStream = new MemoryStream(Encoding.UTF8.GetBytes(TestJsonString));
        var formParseNode = await _formParseNodeFactory.GetRootParseNodeAsync(_formParseNodeFactory.ValidContentType, formStream, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(formParseNode);
        Assert.IsAssignableFrom<FormParseNode>(formParseNode);
    }
    [Fact]
    public async Task ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
    {
        var streamContentType = "application/octet-stream";
        using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestJsonString));
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _formParseNodeFactory.GetRootParseNodeAsync(streamContentType, jsonStream, TestContext.Current.CancellationToken));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"expected a {_formParseNodeFactory.ValidContentType} content type", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ThrowsArgumentNullExceptionForNoContentType(string? contentType)
    {
        using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestJsonString));
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _formParseNodeFactory.GetRootParseNodeAsync(contentType!, jsonStream, TestContext.Current.CancellationToken));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("contentType", exception.ParamName);
    }
}