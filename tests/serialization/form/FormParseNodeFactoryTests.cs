using System.Text;

namespace Microsoft.Kiota.Serialization.Form.Tests;

public class FormParseNodeFactoryTests
{
    private readonly FormParseNodeFactory _formParseNodeFactory = new();
    private const string TestJsonString = "key=value";
    [Fact]
    public void GetsWriterForFormContentType()
    {
        using var formStream = new MemoryStream(Encoding.UTF8.GetBytes(TestJsonString));
        var formParseNode = _formParseNodeFactory.GetRootParseNode(_formParseNodeFactory.ValidContentType, formStream);

        // Assert
        Assert.NotNull(formParseNode);
        Assert.IsAssignableFrom<FormParseNode>(formParseNode);
    }
    [Fact]
    public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
    {
        var streamContentType = "application/octet-stream";
        using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestJsonString));
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _formParseNodeFactory.GetRootParseNode(streamContentType, jsonStream));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"expected a {_formParseNodeFactory.ValidContentType} content type", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsArgumentNullExceptionForNoContentType(string? contentType)
    {
        using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestJsonString));
        var exception = Assert.Throws<ArgumentNullException>(() => _formParseNodeFactory.GetRootParseNode(contentType!, jsonStream));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("contentType", exception.ParamName);
    }
}