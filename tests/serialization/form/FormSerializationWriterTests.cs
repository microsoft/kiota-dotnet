using System.Globalization;
using System.Text;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Form.Tests.Mocks;

namespace Microsoft.Kiota.Serialization.Form.Tests;
public class FormSerializationWriterTests
{
    public FormSerializationWriterTests()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
    }

    [Fact]
    public void WritesSampleObjectValue()
    {
        // Arrange
        var testEntity = new TestEntity()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            WorkDuration = TimeSpan.FromHours(1),
            StartWorkTime = new Time(8, 0, 0),
            BirthDay = new Date(2017, 9, 4),
            Numbers = TestEnum.One | TestEnum.Two,
            DeviceNames = new List<string>
            {
                "device1", "device2"
            },
            AdditionalData = new Dictionary<string, object>
            {
                {"mobilePhone", null!}, // write null value
                {"accountEnabled", false}, // write bool value
                {"jobTitle","Author"}, // write string value
                {"otherPhones", new List<string>{ "123456789", "987654321"} },
                {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                { "decimalValue", 2m},
                { "floatValue", 1.2f},
                { "longValue", 2L},
                { "doubleValue", 2d},
                { "guidValue", Guid.Parse("48d31887-5fad-4d73-a9f5-3c356e68a038")},
                { "intValue", 1}
            }
        };
        using var formSerializerWriter = new FormSerializationWriter();
        // Act
        formSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        // Get the string from the stream.
        var serializedStream = formSerializerWriter.GetSerializedContent();
        using var reader = new StreamReader(serializedStream, Encoding.UTF8);
        var serializedFormString = reader.ReadToEnd();

        // Assert
        var expectedString = "id=48d31887-5fad-4d73-a9f5-3c356e68a038&" +
                                "numbers=one%2Ctwo&" +   // serializes enums
                                "workDuration=PT1H&" +    // Serializes timespans
                                "birthDay=2017-09-04&" + // Serializes dates
                                "startWorkTime=08%3A00%3A00&" + //Serializes times
                                "deviceNames=device1&deviceNames=device2&" + // Serializes collection of scalars using the same key
                                "mobilePhone=null&" + // Serializes null values
                                "accountEnabled=false&" +
                                "jobTitle=Author&" +
                                "otherPhones=123456789&otherPhones=987654321&" + // Serializes collection of scalars using the same key which we present in the AdditionalData
                                "createdDateTime=0001-01-01T00%3A00%3A00.0000000%2B00%3A00&" +
                                "decimalValue=2&" +
                                "floatValue=1.2&" +
                                "longValue=2&" +
                                "doubleValue=2&" +
                                "guidValue=48d31887-5fad-4d73-a9f5-3c356e68a038&" +
                                "intValue=1";
        Assert.Equal(expectedString, serializedFormString);
    }

    [Fact]
    public void DoesNotWritesSampleCollectionOfObjectValues()
    {
        // Arrange
        var testEntity = new TestEntity()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            Numbers = TestEnum.One | TestEnum.Two,
            AdditionalData = new Dictionary<string, object>
            {
                {"mobilePhone",null!}, // write null value
                {"accountEnabled",false}, // write bool value
                {"jobTitle","Author"}, // write string value
                {"createdDateTime", DateTimeOffset.MinValue}, // write date value
            }
        };
        var entityList = new List<TestEntity>() { testEntity };
        using var formSerializerWriter = new FormSerializationWriter();
        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => formSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList));
        Assert.Equal("Form serialization does not support collections.", exception.Message);
    }

    [Fact]
    public void DoesNotWriteNestedObjectValuesInAdditionalData()
    {
        // Arrange
        var testEntity = new TestEntity()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            Numbers = TestEnum.One | TestEnum.Two,
            AdditionalData = new Dictionary<string, object>
            {
                {"nestedEntity", new TestEntity()
                {
                    Id = new Guid().ToString(),
                }} // write nested entity
            }
        };
        using var formSerializerWriter = new FormSerializationWriter();
        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => formSerializerWriter.WriteObjectValue(string.Empty, testEntity));
        Assert.Equal("Form serialization does not support nested objects.", exception.Message);
    }

    [Fact]
    public void WriteBoolValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = true;

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteBoolValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=true", serializedString);
    }

    [Fact]
    public void WriteByteArrayValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new byte[] { 2, 4, 6 };

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteByteArrayValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=AgQG", serializedString);
    }

    [Fact]
    public void WriteByteValue_IsWrittenCorrectly()
    {
        // Arrange
        byte value = 5;

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteByteValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=5", serializedString);
    }

    [Fact]
    public void WriteDateTimeOffsetValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new DateTimeOffset(2024, 11, 30, 15, 35, 45, 987, TimeSpan.FromHours(3));

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteDateTimeOffsetValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=2024-11-30T15%3A35%3A45.9870000%2B03%3A00", serializedString);
    }

    [Fact]
    public void WriteDateValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new Date(2024, 11, 30);

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteDateValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=2024-11-30", serializedString);
    }

    [Fact]
    public void WriteDecimalValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = 36.8m;

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteDecimalValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=36.8", serializedString);
    }

    [Fact]
    public void WriteDoubleValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = 36.8d;

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteDoubleValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=36.8", serializedString);
    }

    [Fact]
    public void WriteFloatValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = 36.8f;

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteFloatValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=36.8", serializedString);
    }

    [Fact]
    public void WriteGuidValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new Guid("3adeb301-58f1-45c5-b820-ae5f4af13c89");

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteGuidValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=3adeb301-58f1-45c5-b820-ae5f4af13c89", serializedString);
    }

    [Fact]
    public void WriteIntegerValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = 25;

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteIntValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=25", serializedString);
    }

    [Fact]
    public void WriteLongValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = long.MaxValue;

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteLongValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=9223372036854775807", serializedString);
    }

    [Fact]
    public void WriteNullValue_IsWrittenCorrectly()
    {
        // Arrange
        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteNullValue("prop1");
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=null", serializedString);
    }

    [Fact]
    public void WriteSByteValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = sbyte.MaxValue;

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteSbyteValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=127", serializedString);
    }

    [Fact]
    public void WriteTimeValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new Time(23, 46, 59);

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteTimeValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=23%3A46%3A59", serializedString);
    }

    [Fact]
    public void WriteTimeSpanValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = new TimeSpan(756, 4, 6, 8, 10);

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteTimeSpanValue("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=P756DT4H6M8.01S", serializedString);
    }

    [Fact]
    public void WriteAdditionalData_AreWrittenCorrectly()
    {
        // Arrange
        var additionalData = new Dictionary<string, object>()
        {
            { "prop1", "value1" },
            { "prop2", 2 },
            { "prop3", true },
            { "prop4", 2.25d },
            { "prop5", 3.14f },
            { "prop6", 4L },
            { "prop7", 5m },
            { "prop8", new DateTimeOffset(2024, 11, 30, 15, 35, 45, 987, TimeSpan.FromHours(3)) },
            { "prop9", new Date(2024, 11, 30) },
            { "prop10", new Time(23, 46, 59) },
            { "prop11", new TimeSpan(756, 4, 6, 8, 10) },
            { "prop12", new byte[] { 2, 4, 6 } },
            { "prop13", new Guid("3adeb301-58f1-45c5-b820-ae5f4af13c89") },
            { "prop14", sbyte.MaxValue }
        };

        using var formSerializationWriter = new FormSerializationWriter();

        // Act and Assert
        formSerializationWriter.WriteAdditionalData(additionalData);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=value1&prop2=2&prop3=true&prop4=2.25&prop5=3.14&prop6=4&prop7=5&prop8=2024-11-30T15%3A35%3A45.9870000%2B03%3A00&prop9=2024-11-30&prop10=23%3A46%3A59&prop11=P756DT4H6M8.01S&prop12=AgQG&prop13=3adeb301-58f1-45c5-b820-ae5f4af13c89&prop14=127", serializedString);
    }

    [Fact]
    public void WriteEnumValue_IsWrittenCorrectly()
    {
        // Arrange
        var value = TestEnum.Sixteen;

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteEnumValue<TestEnum>("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=sixteen", serializedString);
    }

    [Fact]
    public void WriteCollectionOfEnumValues_IsWrittenCorrectly()
    {
        // Arrange
        var value = new List<TestEnum?> { TestEnum.Sixteen, TestEnum.Two };

        using var formSerializationWriter = new FormSerializationWriter();

        // Act
        formSerializationWriter.WriteCollectionOfEnumValues("prop1", value);
        var contentStream = formSerializationWriter.GetSerializedContent();
        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        var serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=sixteen%2Ctwo", serializedString);
    }
}
