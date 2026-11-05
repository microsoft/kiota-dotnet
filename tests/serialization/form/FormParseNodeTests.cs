using System.Globalization;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Form.Tests.Mocks;

namespace Microsoft.Kiota.Serialization.Form.Tests;

public class FormParseNodeTests
{
    public FormParseNodeTests()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
    }

    private const string TestUserForm = "displayName=Megan+Bowen&" +
                                        "numbers=one,two,thirtytwo&" +
                                        "givenName=Megan&" +
                                        "accountEnabled=true&" +
                                        "createdDateTime=2017-07-29T03:07:25Z&" +
                                        "jobTitle=Auditor&" +
                                        "mail=MeganB@M365x214355.onmicrosoft.com&" +
                                        "mobilePhone=null&" +
                                        "officeLocation=null&" +
                                        "preferredLanguage=en-US&" +
                                        "surname=Bowen&" +
                                        "workDuration=PT1H&" +
                                        "startWorkTime=08:00:00.0000000&" +
                                        "endWorkTime=17:00:00.0000000&" +
                                        "userPrincipalName=MeganB@M365x214355.onmicrosoft.com&" +
                                        "birthDay=2017-09-04&" +
                                        "deviceNames=device1&deviceNames=device2&" + //collection property
                                        "otherPhones=123456789&otherPhones=987654321&" + //collection property for additionalData
                                        "id=48d31887-5fad-4d73-a9f5-3c356e68a038";

    [Fact]
    public void GetsEntityValueFromForm()
    {
        // Arrange
        var formParseNode = new FormParseNode(TestUserForm);
        // Act
        var testEntity = formParseNode.GetObjectValue<TestEntity>(x => new TestEntity());
        // Assert
        Assert.NotNull(testEntity);
        Assert.Null(testEntity.OfficeLocation);
        Assert.NotEmpty(testEntity.AdditionalData);
        Assert.Equal(2, testEntity.DeviceNames?.Count);// collection is deserialized
        Assert.True(testEntity.AdditionalData.ContainsKey("jobTitle"));
        Assert.True(testEntity.AdditionalData.ContainsKey("mobilePhone"));
        Assert.True(testEntity.AdditionalData.ContainsKey("otherPhones"));
        Assert.Equal("true", testEntity.AdditionalData["accountEnabled"]);
        Assert.Equal("Auditor", testEntity.AdditionalData["jobTitle"]);
        Assert.Equal("123456789,987654321", testEntity.AdditionalData["otherPhones"]);
        Assert.Equal("48d31887-5fad-4d73-a9f5-3c356e68a038", testEntity.Id);
        Assert.Equal(TestEnum.One | TestEnum.Two, testEntity.Numbers); // Unknown enum value is not included
        Assert.Equal(TimeSpan.FromHours(1), testEntity.WorkDuration); // Parses timespan values
        Assert.Equal(new Time(8, 0, 0).ToString(), testEntity.StartWorkTime.ToString());// Parses time values
        Assert.Equal(new Time(17, 0, 0).ToString(), testEntity.EndWorkTime.ToString());// Parses time values
        Assert.Equal(new Date(2017, 9, 4).ToString(), testEntity.BirthDay.ToString());// Parses date values
    }

    [Fact]
    public void GetCollectionOfNumberPrimitiveValuesFromForm()
    {
        string TestFormData = "numbers=1&" +
                              "numbers=2&" +
                              "numbers=3&";
        var formParseNode = new FormParseNode(TestFormData);
        var numberNode = formParseNode.GetChildNode("numbers");
        var numberCollection = numberNode?.GetCollectionOfPrimitiveValues<int?>();
        Assert.NotNull(numberCollection);
        Assert.Equal(3, numberCollection.Count());
        Assert.Equal(1, numberCollection.First());
        var numberCollectionAsStrings = numberNode?.GetCollectionOfPrimitiveValues<string?>();
        Assert.NotNull(numberCollectionAsStrings);
        Assert.Equal(3, numberCollectionAsStrings.Count());
        Assert.Equal("1", numberCollectionAsStrings.First());
        var numberCollectionAsShort = numberNode?.GetCollectionOfPrimitiveValues<sbyte?>();
        Assert.NotNull(numberCollectionAsShort);
        Assert.Equal(3, numberCollectionAsShort.Count());
        Assert.Equal((sbyte)1, numberCollectionAsShort.First());
        var numberCollectionAsDouble = numberNode?.GetCollectionOfPrimitiveValues<double?>();
        Assert.NotNull(numberCollectionAsDouble);
        Assert.Equal(3, numberCollectionAsDouble.Count());
        Assert.Equal((double)1, numberCollectionAsDouble.First());
        var numberCollectionAsFloat = numberNode?.GetCollectionOfPrimitiveValues<float?>();
        Assert.NotNull(numberCollectionAsFloat);
        Assert.Equal(3, numberCollectionAsFloat.Count());
        Assert.Equal((float)1, numberCollectionAsFloat.First());
        var numberCollectionAsDecimal = numberNode?.GetCollectionOfPrimitiveValues<decimal?>();
        Assert.NotNull(numberCollectionAsDecimal);
        Assert.Equal(3, numberCollectionAsDecimal.Count());
        Assert.Equal((decimal)1, numberCollectionAsDecimal.First());
    }

    [Fact]
    public void GetCollectionOfBooleanPrimitiveValuesFromForm()
    {
        string TestFormData = "bools=true&" +
                              "bools=false";
        var formParseNode = new FormParseNode(TestFormData);
        var numberNode = formParseNode.GetChildNode("bools");
        var numberCollection = numberNode?.GetCollectionOfPrimitiveValues<bool?>();
        Assert.NotNull(numberCollection);
        Assert.Equal(2, numberCollection.Count());
        Assert.Equal(true, numberCollection.First());
        Assert.Equal(false, numberCollection.Last());
    }

    [Fact]
    public void GetCollectionOfBooleanPrimitiveValuesWithInvalidEntriesFromForm()
    {
        string testFormData = "bools=true&" +
                              "bools=invalid&" +
                              "bools=null&" +
                              "bools=false";
        var formParseNode = new FormParseNode(testFormData);
        var boolsNode = formParseNode.GetChildNode("bools");
        var collection = boolsNode?.GetCollectionOfPrimitiveValues<bool?>()?.ToList();
        Assert.NotNull(collection);
        Assert.Equal(4, collection.Count);
        Assert.True(collection[0]);
        Assert.Null(collection[1]);
        Assert.Null(collection[2]);
        Assert.False(collection[3]);
    }

    [Fact]
    public void GetCollectionOfGuidPrimitiveValuesFromForm()
    {
        string TestFormData = "ids=48d31887-5fad-4d73-a9f5-3c356e68a038&" +
                              "ids=48d31887-5fad-4d73-a9f5-3c356e68a038";
        var formParseNode = new FormParseNode(TestFormData);
        var numberNode = formParseNode.GetChildNode("ids");
        var numberCollection = numberNode?.GetCollectionOfPrimitiveValues<Guid?>();
        Assert.NotNull(numberCollection);
        Assert.Equal(2, numberCollection.Count());
        Assert.Equal(Guid.Parse("48d31887-5fad-4d73-a9f5-3c356e68a038"), numberCollection.First());
    }

    [Fact]
    public void GetCollectionOfObjectValuesFromForm()
    {
        var formParseNode = new FormParseNode(TestUserForm);
        Assert.Throws<InvalidOperationException>(() => formParseNode.GetCollectionOfObjectValues<TestEntity>(static x => new TestEntity()));
    }

    [Fact]
    public void ReturnsDefaultIfChildNodeDoesNotExist()
    {
        // Arrange
        var rootParseNode = new FormParseNode(TestUserForm);
        // Try to get an imaginary node value
        var imaginaryNode = rootParseNode.GetChildNode("imaginaryNode");
        // Assert
        Assert.Null(imaginaryNode);
    }

    [Fact]
    public void FormParseNode_GetBoolValue()
    {
        // Arrange
        var text = "true";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetBoolValue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FormParseNode_GetBoolValue_False()
    {
        // Arrange
        var text = "false";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetBoolValue();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FormParseNode_GetBoolValue_ReturnsNullForInvalid()
    {
        // Arrange & Act & Assert
        Assert.Null(new FormParseNode("invalid").GetBoolValue());
        Assert.Null(new FormParseNode("null").GetBoolValue());
        Assert.Null(new FormParseNode("").GetBoolValue());
        Assert.Null(new FormParseNode("123").GetBoolValue());
    }

    [Fact]
    public void FormParseNode_GetByteArrayValue()
    {
        // Arrange
        var text = "dGV4dA==";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetByteArrayValue();
        // Assert
        Assert.Equal([0x74, 0x65, 0x78, 0x74], result);
    }

    [Fact]
    public void FormParseNode_GetByteArrayValue_WithEmbeddedWhitespace()
    {
        // Arrange & Act
        var urlEncodedSpaces = new FormParseNode("dG%20%20V4dA%3D%3D").GetByteArrayValue();
        var embeddedWhitespace = new FormParseNode("dG \t\r\nV4dA==").GetByteArrayValue();

        // Assert
        Assert.Equal([0x74, 0x65, 0x78, 0x74], urlEncodedSpaces);
        Assert.Equal([0x74, 0x65, 0x78, 0x74], embeddedWhitespace);
    }

    [Fact]
    public void FormParseNode_GetByteArrayValue_WithPaddedBase64()
    {
        // Arrange & Act & Assert
        Assert.Equal([0x61], new FormParseNode("YQ==").GetByteArrayValue());
        Assert.Equal([0x61, 0x62], new FormParseNode("YWI=").GetByteArrayValue());
        Assert.Equal([0x61, 0x62, 0x63], new FormParseNode("YWJj").GetByteArrayValue());
        Assert.Equal([0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64],
            new FormParseNode("SGVsbG8gV29ybGQ=").GetByteArrayValue());
    }

    [Fact]
    public void FormParseNode_GetByteArrayValue_NullAndInvalid()
    {
        // Arrange & Act & Assert
        // Null or empty tokens return null
        Assert.Null(new FormParseNode("null").GetByteArrayValue());
        Assert.Null(new FormParseNode("").GetByteArrayValue());

        // Malformed or non-standard Base64 payloads return null upon FormatException
        Assert.Null(new FormParseNode("!invalid-base64!").GetByteArrayValue());
        Assert.Null(new FormParseNode("dGV4dA").GetByteArrayValue()); // Unpadded Base64
        Assert.Null(new FormParseNode("-_-_").GetByteArrayValue()); // URL-safe characters (- and _)
    }

    [Fact]
    public void FormParseNode_GetByteValue()
    {
        // Arrange
        var text = "1";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetByteValue();

        // Assert
        Assert.Equal((byte)1, result);
    }

    [Fact]
    public void FormParseNode_GetDateTimeOffsetValue()
    {
        // Arrange
        var text = "2021-11-30T12:24:36+03:00";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetDateTimeOffsetValue();

        // Assert
        Assert.Equal(new DateTimeOffset(2021, 11, 30, 12, 24, 36, TimeSpan.FromHours(3)), result);
    }

    [Fact]
    public void FormParseNode_GetDateValue()
    {
        // Arrange
        var text = "2021-11-30";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetDateValue();

        // Assert
        Assert.Equal(new Date(2021, 11, 30), result);
    }

    [Fact]
    public void FormParseNode_GetTimeSpanValue()
    {
        // Arrange
        var text = "P756DT4H6M8.01S";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetTimeSpanValue();

        // Assert
        Assert.Equal(new TimeSpan(756, 4, 6, 8, 10), result);
    }

    [Fact]
    public void FormParseNode_GetTimeSpanValue_NullAndInvalid()
    {
        // Arrange & Act & Assert
        Assert.Null(new FormParseNode("null").GetTimeSpanValue());
        Assert.Null(new FormParseNode("").GetTimeSpanValue());
        Assert.Null(new FormParseNode("invalid-duration").GetTimeSpanValue());
        Assert.Null(new FormParseNode("P10675200D").GetTimeSpanValue());
    }

    [Fact]
    public void GetCollectionOfTimeSpanPrimitiveValuesWithInvalidEntriesFromForm()
    {
        string testFormData = "durations=PT1H&" +
                              "durations=null&" +
                              "durations=invalid&" +
                              "durations=P10675200D&" +
                              "durations=PT2H";
        var formParseNode = new FormParseNode(testFormData);
        var durationsNode = formParseNode.GetChildNode("durations");
        var collection = durationsNode?.GetCollectionOfPrimitiveValues<TimeSpan?>()?.ToList();
        Assert.NotNull(collection);
        Assert.Equal(5, collection.Count);
        Assert.Equal(TimeSpan.FromHours(1), collection[0]);
        Assert.Null(collection[1]);
        Assert.Null(collection[2]);
        Assert.Null(collection[3]);
        Assert.Equal(TimeSpan.FromHours(2), collection[4]);
    }

    [Fact]
    public void FormParseNode_GetTimeValue()
    {
        // Arrange
        var text = "12:24:36";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetTimeValue();

        // Assert
        Assert.Equal(new Time(12, 24, 36), result);
    }

    [Fact]
    public void FormParseNode_GetGuidValue()
    {
        // Arrange
        var text = "f4b3b8f4-6f4d-4f1f-8f4d-8f4b3b8f4d1f";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetGuidValue();

        // Assert
        Assert.Equal(new Guid("f4b3b8f4-6f4d-4f1f-8f4d-8f4b3b8f4d1f"), result);
    }

    [Fact]
    public void FormParseNode_GetIntValue()
    {
        // Arrange
        var text = "1";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetIntValue();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void FormParseNode_GetLongValue()
    {
        // Arrange
        var text = "1";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetLongValue();

        // Assert
        Assert.Equal(1L, result);
    }

    [Fact]
    public void FormParseNode_GetSbyteValue()
    {
        // Arrange
        var text = "1";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetSbyteValue();

        // Assert
        Assert.Equal((sbyte)1, result);
    }

    [Fact]
    public void FormParseNode_GetStringValue()
    {
        // Arrange
        var text = "text";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetStringValue();

        // Assert
        Assert.Equal("text", result);
    }

    [Fact]
    public void FormParseNode_GetDecimalValue()
    {
        // Arrange
        var text = "1.1";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetDecimalValue();

        // Assert
        Assert.Equal(1.1m, result);
    }

    [Fact]
    public void FormParseNode_GetDoubleValue()
    {
        // Arrange
        var text = "1.1";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetDoubleValue();

        // Assert
        Assert.Equal(1.1, result);
    }

    [Fact]
    public void FormParseNode_GetFloatValue()
    {
        // Arrange
        var text = "1.1";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetFloatValue();

        // Assert
        Assert.Equal(1.1f, result);
    }

    [Fact]
    public void FormParseNode_GetEnumFromInteger()
    {
        // Arange
        var text = "8";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetEnumValue<TestEnum>();

        // Assert
        Assert.Equal(TestEnum.Eight, result);
    }

    [Fact]
    public void FormParseNode_GetEnumFromString()
    {
        // Arange
        var text = "Sixteen";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetEnumValue<TestEnum>();

        // Assert
        Assert.Equal(TestEnum.Sixteen, result);
    }

    [Fact]
    public void FormParseNode_GetEnumsFromInteger()
    {
        // Arange
        var text = "8,16";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetCollectionOfEnumValues<TestEnum>();

        // Assert
        Assert.Equal([TestEnum.Eight, TestEnum.Sixteen], result);
    }

    [Fact]
    public void FormParseNode_GetEnumsFromString()
    {
        // Arange
        var text = "Sixteen,Eight";
        var parseNode = new FormParseNode(text);

        // Act
        var result = parseNode.GetCollectionOfEnumValues<TestEnum>();

        // Assert
        Assert.Equal([TestEnum.Sixteen, TestEnum.Eight], result);
    }

    [Fact]
    public void GetCollectionOfLongPrimitiveValuesFromForm()
    {
        string TestFormData = "values=1000000000&" +
                              "values=2000000000&" +
                              "values=3000000000&";
        var formParseNode = new FormParseNode(TestFormData);
        var node = formParseNode.GetChildNode("values");
        Assert.NotNull(node);
        var collection = node.GetCollectionOfPrimitiveValues<long?>();
        Assert.NotNull(collection);
        Assert.Equal(3, collection.Count());
        Assert.Equal(1000000000L, collection.First());
    }

    [Fact]
    public void GetCollectionOfDateTimeOffsetPrimitiveValuesFromForm()
    {
        string TestFormData = "values=2021-11-30T12%3A24%3A36%2B03%3A00&" +
                              "values=2022-06-15T08%3A00%3A00Z&";
        var formParseNode = new FormParseNode(TestFormData);
        var node = formParseNode.GetChildNode("values");
        Assert.NotNull(node);
        var collection = node.GetCollectionOfPrimitiveValues<DateTimeOffset?>();
        Assert.NotNull(collection);
        var items = collection.ToArray();
        Assert.Equal(2, items.Length);
        Assert.Equal(new DateTimeOffset(2021, 11, 30, 12, 24, 36, TimeSpan.FromHours(3)), items[0]);
        Assert.Equal(new DateTimeOffset(2022, 6, 15, 8, 0, 0, TimeSpan.Zero), items[1]);
    }

    [Fact]
    public void GetCollectionOfDatePrimitiveValuesFromForm()
    {
        string TestFormData = "values=2021-11-30&" +
                              "values=2022-06-15&";
        var formParseNode = new FormParseNode(TestFormData);
        var node = formParseNode.GetChildNode("values");
        Assert.NotNull(node);
        var collection = node.GetCollectionOfPrimitiveValues<Date?>();
        Assert.NotNull(collection);
        var items = collection.ToArray();
        Assert.Equal(2, items.Length);
        Assert.Equal(new Date(2021, 11, 30), items[0]);
        Assert.Equal(new Date(2022, 6, 15), items[1]);
    }
}
