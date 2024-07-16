using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Text.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Xunit;

namespace Microsoft.Kiota.Serialization.Text.Tests;

public class TextSerializationWriterTests
{
    public TextSerializationWriterTests()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
    }

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

    [Fact]
    public void WriteBoolValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = true;

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteBoolValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("True", serializedString);
    }

    [Fact]
    public void WriteByteArrayValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new byte[] { 2, 4, 6 };

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteByteArrayValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("AgQG", serializedString);
    }

    [Fact]
    public void WriteByteValue_IsWrittenCorrectly()
    {
        // Arrange
        byte value = 5;

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteByteValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("5", serializedString);
    }

    [Fact]
    public void WriteDateTimeOffsetValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new DateTimeOffset(2024,11,30,15,35,45,987, TimeSpan.FromHours(3));

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteDateTimeOffsetValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("2024-11-30T15:35:45.9870000+03:00", serializedString);
    }

    [Fact]
    public void WriteDateValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new Date(2024, 11, 30);

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteDateValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("2024-11-30", serializedString);
    }

    [Fact]
    public void WriteDecimalValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = 36.8m;

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteDecimalValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("36.8", serializedString);
    }

    [Fact]
    public void WriteDoubleValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = 36.8d;

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteDoubleValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("36.8", serializedString);
    }

    [Fact]
    public void WriteFloatValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = 36.8f;

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteFloatValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("36.8", serializedString);
    }

    [Fact]
    public void WriteGuidValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new Guid("3adeb301-58f1-45c5-b820-ae5f4af13c89");

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteGuidValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("3adeb301-58f1-45c5-b820-ae5f4af13c89", serializedString);
    }

    [Fact]
    public void WriteIntegerValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = 25;

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteIntValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("25", serializedString);
    }

    [Fact]
    public void WriteLongValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = long.MaxValue;

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteLongValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("9223372036854775807", serializedString);
    }

    [Fact]
    public void WriteNullValue_IsWrittenCorrectly()
    {
        // Arrange
        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteNullValue(null);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("null", serializedString);
    }

    [Fact]
    public void WriteSByteValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = sbyte.MaxValue;

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteSbyteValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("127", serializedString);
    }

    [Fact]
    public void WriteTimeValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new Time(23, 46, 59);

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteTimeValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("23:46:59", serializedString);
    }

    [Fact]
    public void WriteTimeSpanValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new TimeSpan(756, 4, 6, 8, 10);

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteTimeSpanValue(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("P756DT4H6M8.01S", serializedString);
    }

    [Fact]
    public void WriteAdditionalData_ThrowsInvalidOperationException()
    {
        // Arrange
        var additionalData = new Dictionary<string, object>();

        using var textSerializationWriter = new TextSerializationWriter();

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => textSerializationWriter.WriteAdditionalData(additionalData));
    }

    [Fact]
    public void WriteEnumValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = TestEnum.FirstItem;

        using var textSerializationWriter = new TextSerializationWriter();

        // Act
        textSerializationWriter.WriteEnumValue<TestEnum>(null, value);
        var contentStream = textSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("firstItem", serializedString);
    }

    [Fact]
    public void WriteCollectionOfEnumValues_ThrowsInvalidOperationException()
    {
        // Arrange
        var value = new List<TestEnum?> { TestEnum.FirstItem, TestEnum.SecondItem };

        using var textSerializationWriter = new TextSerializationWriter();

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => textSerializationWriter.WriteCollectionOfEnumValues(null, value));
    }
}
