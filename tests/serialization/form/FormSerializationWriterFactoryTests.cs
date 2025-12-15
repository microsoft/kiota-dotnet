namespace Microsoft.Kiota.Serialization.Form.Tests;

public class FormSerializationWriterFactoryTests
{
    private readonly FormSerializationWriterFactory _formSerializationFactory = new();

    [Fact]
    public void GetsWriterForFormContentType()
    {
        var formWriter = _formSerializationFactory.GetSerializationWriter(_formSerializationFactory.ValidContentType);

        // Assert
        Assert.NotNull(formWriter);
        Assert.IsAssignableFrom<FormSerializationWriter>(formWriter);
    }

    [Fact]
    public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
    {
        var streamContentType = "application/octet-stream";
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _formSerializationFactory.GetSerializationWriter(streamContentType));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"expected a {_formSerializationFactory.ValidContentType} content type", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsArgumentNullExceptionForNoContentType(string? contentType)
    {
        var exception = Assert.Throws<ArgumentNullException>(() => _formSerializationFactory.GetSerializationWriter(contentType!));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("contentType", exception.ParamName);
    }
}
