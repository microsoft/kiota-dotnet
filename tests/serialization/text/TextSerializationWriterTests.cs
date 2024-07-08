using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.Kiota.Serialization.Text.Tests;

public class TextSerializationWriterTests
{
    [Fact]
    public void WritesStringValue()
    {
        // Arrange
        var value = "This is a string value";

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteStringValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal(value, serializedString);
    }

    [Fact]
    public void StreamIsReadableAfterDispose()
    {
        // Arrange
        var value = "This is a string value";

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteStringValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        // Dispose the writer
        textSerializationWriter.Dispose();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal(value, serializedString);
    }
}
