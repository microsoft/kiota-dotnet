using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Form.Tests.Mocks;

namespace Microsoft.Kiota.Serialization.Form.Tests;
public class FormParseNodeTests
{
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
                                        "deviceNames=device1&deviceNames=device2&"+ //collection property
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
        Assert.Equal("true",testEntity.AdditionalData["accountEnabled"]);
        Assert.Equal("Auditor", testEntity.AdditionalData["jobTitle"]);
        Assert.Equal("123456789,987654321", testEntity.AdditionalData["otherPhones"]);
        Assert.Equal("48d31887-5fad-4d73-a9f5-3c356e68a038", testEntity.Id);
        Assert.Equal(TestEnum.One | TestEnum.Two, testEntity.Numbers ); // Unknown enum value is not included
        Assert.Equal(TimeSpan.FromHours(1), testEntity.WorkDuration); // Parses timespan values
        Assert.Equal(new Time(8,0,0).ToString(),testEntity.StartWorkTime.ToString());// Parses time values
        Assert.Equal(new Time(17, 0, 0).ToString(), testEntity.EndWorkTime.ToString());// Parses time values
        Assert.Equal(new Date(2017,9,4).ToString(), testEntity.BirthDay.ToString());// Parses date values
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
}
