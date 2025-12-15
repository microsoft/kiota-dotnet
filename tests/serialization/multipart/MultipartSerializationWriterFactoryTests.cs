using System;
using Xunit;

namespace Microsoft.Kiota.Serialization.Multipart.Tests;

public class MultipartSerializationWriterFactoryTests
{
    private readonly MultipartSerializationWriterFactory _multipartSerializationFactory;

    public MultipartSerializationWriterFactoryTests()
    {
        _multipartSerializationFactory = new MultipartSerializationWriterFactory();
    }

    [Fact]
    public void GetsWriterForMultipartContentType()
    {
        var multipartWriter = _multipartSerializationFactory.GetSerializationWriter(_multipartSerializationFactory.ValidContentType);

        // Assert
        Assert.NotNull(multipartWriter);
        Assert.IsAssignableFrom<MultipartSerializationWriter>(multipartWriter);
    }

    [Fact]
    public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
    {
        var streamContentType = "application/octet-stream";
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _multipartSerializationFactory.GetSerializationWriter(streamContentType));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"expected a {_multipartSerializationFactory.ValidContentType} content type", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsArgumentNullExceptionForNoContentType(string? contentType)
    {
        var exception = Assert.Throws<ArgumentNullException>(() => _multipartSerializationFactory.GetSerializationWriter(contentType!));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("contentType", exception.ParamName);
    }
}
