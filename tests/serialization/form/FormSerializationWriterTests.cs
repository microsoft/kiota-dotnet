using System.Text;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Form.Tests.Mocks;

namespace Microsoft.Kiota.Serialization.Form.Tests;
public class FormSerializationWriterTests
{
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
        formSerializerWriter.WriteObjectValue(string.Empty,testEntity);
        // Get the string from the stream.
        var serializedStream = formSerializerWriter.GetSerializedContent();
        using var reader = new StreamReader(serializedStream, Encoding.UTF8);
        var serializedFormString = reader.ReadToEnd();
        
        // Assert
        var expectedString =    "id=48d31887-5fad-4d73-a9f5-3c356e68a038&" +
                                "numbers=one%2Ctwo&"+   // serializes enums
                                "workDuration=PT1H&"+    // Serializes timespans
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
        Assert.Equal("Form serialization does not support nested objects.",exception.Message);
    }
}
