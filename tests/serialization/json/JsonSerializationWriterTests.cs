using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json.Tests.Converters;
using Microsoft.Kiota.Serialization.Json.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Json.Tests
{
    public class JsonSerializationWriterTests
    {
        public JsonSerializationWriterTests()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
        }

        [Fact]
        public void WritesSampleObjectValue()
        {
            var nullJsonElement = JsonDocument.Parse("null").RootElement;

            // Arrange
            var testEntity = new TestEntity()
            {
                Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
                WorkDuration = TimeSpan.FromHours(1),
                StartWorkTime = new Time(8, 0, 0),
                BirthDay = new Date(2017, 9, 4),
                HeightInMetres = 1.80m,
                AdditionalData = new Dictionary<string, object>
                {
                    {"mobilePhone",nullJsonElement}, // write null value
                    {"accountEnabled",false}, // write bool value
                    {"jobTitle","Author"}, // write string value
                    {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                    {"weightInKgs", 51.80m}, // write weigth
                    {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                    {"endDateTime", new DateTime(2023,03,14,0,0,0,DateTimeKind.Utc) }, // ensure the DateTime doesn't crash
                    {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
                    {"anonymousObject", new {Value1 = true, Value2 = "", Value3 = new List<string>{ "Value3.1", "Value3.2"}}}, // write nested object value
                    {"dictionaryString", new Dictionary<string, string>{{"91bbe8e2-09b2-482b-a90e-00f8d7e81636", "b7992f48-a51b-41a1-ace5-4cebb7f111d0"}, { "ed64c116-2776-4012-94d1-a348b9d241bd", "55e1b4d0-2959-4c71-89b5-385ba5338a1c" }, }}, // write a Dictionary
                    {"dictionaryTestEntity", new Dictionary<string, TestEntity>{{ "dd476fc9-7e97-4a4e-8d40-6c3de7432eb3", new TestEntity { Id = "dd476fc9-7e97-4a4e-8d40-6c3de7432eb3" } }, { "ffa5c351-7cf5-43df-9b55-e12455cf6eb2", new TestEntity { Id = "ffa5c351-7cf5-43df-9b55-e12455cf6eb2" } }, }}, // write a Dictionary
                }
            };

            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "{" +
                                 "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                                 "\"workDuration\":\"PT1H\"," +    // Serializes timespans
                                 "\"birthDay\":\"2017-09-04\"," + // Serializes dates
                                 "\"heightInMetres\":1.80," +
                                 "\"startWorkTime\":\"08:00:00\"," + //Serializes times
                                 "\"mobilePhone\":null," +
                                 "\"accountEnabled\":false," +
                                 "\"jobTitle\":\"Author\"," +
                                 "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                                 "\"weightInKgs\":51.80," +
                                 "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                                 "\"endDateTime\":\"2023-03-14T00:00:00+00:00\"," +
                                 "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}," +
                                 "\"anonymousObject\":{\"Value1\":true,\"Value2\":\"\",\"Value3\":[\"Value3.1\",\"Value3.2\"]}," +
                                 "\"dictionaryString\":{\"91bbe8e2-09b2-482b-a90e-00f8d7e81636\":\"b7992f48-a51b-41a1-ace5-4cebb7f111d0\",\"ed64c116-2776-4012-94d1-a348b9d241bd\":\"55e1b4d0-2959-4c71-89b5-385ba5338a1c\"}," +
                                 "\"dictionaryTestEntity\":{\"dd476fc9-7e97-4a4e-8d40-6c3de7432eb3\":{\"id\":\"dd476fc9-7e97-4a4e-8d40-6c3de7432eb3\"},\"ffa5c351-7cf5-43df-9b55-e12455cf6eb2\":{\"id\":\"ffa5c351-7cf5-43df-9b55-e12455cf6eb2\"}}" +
                                 "}";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WritesSampleObjectValueWithJsonElementAdditionalData()
        {
            var nullJsonElement = JsonDocument.Parse("null").RootElement;
            var arrayJsonElement = JsonDocument.Parse("[\"+1 412 555 0109\"]").RootElement;
            var objectJsonElement = JsonDocument.Parse("{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}").RootElement;

            // Arrange
            var testEntity = new TestEntity()
            {
                Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
                WorkDuration = TimeSpan.FromHours(1),
                StartWorkTime = new Time(8, 0, 0),
                BirthDay = new Date(2017, 9, 4),
                AdditionalData = new Dictionary<string, object>
                {
                    {"mobilePhone", nullJsonElement}, // write null value
                    {"accountEnabled",false}, // write bool value
                    {"jobTitle","Author"}, // write string value
                    {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                    {"businessPhones", arrayJsonElement }, // write collection of primitives value
                    {"manager", objectJsonElement }, // write nested object value
                }
            };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "{" +
                                 "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                                 "\"workDuration\":\"PT1H\"," +    // Serializes timespans
                                 "\"birthDay\":\"2017-09-04\"," + // Serializes dates
                                 "\"startWorkTime\":\"08:00:00\"," + //Serializes times
                                 "\"mobilePhone\":null," +
                                 "\"accountEnabled\":false," +
                                 "\"jobTitle\":\"Author\"," +
                                 "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                                 "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                                 "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}" +
                                 "}";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WritesSampleCollectionOfObjectValues()
        {
            var nullJsonElement = JsonDocument.Parse("null").RootElement;

            // Arrange
            var testEntity = new TestEntity()
            {
                Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
                Numbers = TestEnum.One | TestEnum.Two,
                TestNamingEnum = TestNamingEnum.Item2SubItem1,
                AdditionalData = new Dictionary<string, object>
                {
                    {"mobilePhone",nullJsonElement}, // write null value
                    {"accountEnabled",false}, // write bool value
                    {"jobTitle","Author"}, // write string value
                    {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                    {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                    {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
                }
            };
            var entityList = new List<TestEntity>() { testEntity };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "[{" +
                                 "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                                 "\"numbers\":\"One,Two\"," +
                                 "\"testNamingEnum\":\"Item2:SubItem1\"," +
                                 "\"mobilePhone\":null," +
                                 "\"accountEnabled\":false," +
                                 "\"jobTitle\":\"Author\"," +
                                 "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                                 "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                                 "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}" +
                                 "}]";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void DoesntWriteUnsupportedTypes_NonStringKeyedDictionary()
        {
            // Arrange
            var testEntity = new TestEntity()
            {
                AdditionalData = new Dictionary<string, object>
                {
                    {"nonStringKeyedDictionary", new Dictionary<int, string>{{ 1, "one" }, { 2, "two" }}}
                }
            };

            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity));
            Assert.Equal("Error serializing dictionary value with key nonStringKeyedDictionary, only string keyed dictionaries are supported.", exception.Message);
        }

        [Fact]
        public void WritesEnumValuesAsCamelCasedIfNotEscaped()
        {
            // Arrange
            var testEntity = new TestEntity()
            {
                TestNamingEnum = TestNamingEnum.Item1,
            };
            var entityList = new List<TestEntity>() { testEntity };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "[{" +
                                 "\"testNamingEnum\":\"Item1\"" + // Camel Cased
                                 "}]";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WritesEnumValuesAsDescribedIfEscaped()
        {
            // Arrange
            var testEntity = new TestEntity()
            {
                TestNamingEnum = TestNamingEnum.Item2SubItem1,
            };
            var entityList = new List<TestEntity>() { testEntity };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "[{" +
                                 "\"testNamingEnum\":\"Item2:SubItem1\"" + // Appears same as attribute
                                 "}]";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WriteGuidUsingConverter()
        {
            // Arrange
            var id = Guid.NewGuid();
            var testEntity = new ConverterTestEntity { Id = id };
            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                Converters = { new JsonGuidConverter() }
            };
            var serializationContext = new KiotaJsonSerializationContext(serializerOptions);
            using var jsonSerializerWriter = new JsonSerializationWriter(serializationContext);

            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = $"{{\"id\":\"{id:N}\"}}";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void ForwardsOptionsToWriterFromSerializationContext()
        {
            // Arrange
            var testEntity = new TestEntity
            {
                Id = "testId",
                AdditionalData = new Dictionary<string, object>()
                {
                    {"href", "https://graph.microsoft.com/users/{user-id}"},
                    {"unicodeName", "你好"}
                }
            };
            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var serializationContext = new KiotaJsonSerializationContext(serializerOptions);
            using var jsonSerializerWriter = new JsonSerializationWriter(serializationContext);

            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            const string expectedString = "{\n  \"id\": \"testId\",\n  \"href\": \"https://graph.microsoft.com/users/{user-id}\",\n  \"unicodeName\": \"你好\"\n}";
            Assert.Contains("\n", serializedJsonString); // string is indented and not escaped
            Assert.Contains("你好", serializedJsonString); // string is indented and not escaped
            Assert.Equal(expectedString, serializedJsonString.Replace("\r", string.Empty)); // string is indented and not escaped
        }

        [Fact]
        public void UsesDefaultOptionsToWriterFromSerializationContext()
        {
            // Arrange
            var testEntity = new TestEntity
            {
                Id = "testId",
                AdditionalData = new Dictionary<string, object>()
                {
                    {"href", "https://graph.microsoft.com/users/{user-id}"},
                    {"unicodeName", "你好"}
                }
            };
            using var jsonSerializerWriter = new JsonSerializationWriter(new KiotaJsonSerializationContext());

            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = $"{{\"id\":\"testId\",\"href\":\"https://graph.microsoft.com/users/{{user-id}}\",\"unicodeName\":\"\\u4F60\\u597D\"}}";
            Assert.DoesNotContain("\n", serializedJsonString); // string is not indented and not escaped
            Assert.DoesNotContain("你好", serializedJsonString); // string is not indented and not escaped
            Assert.Contains("\\u4F60\\u597D", serializedJsonString); // string is not indented and not escaped
            Assert.Equal(expectedString, serializedJsonString); // string is indented and not escaped
        }
        [Fact]
        public void WriteGuidUsingNoConverter()
        {
            // Arrange
            var id = Guid.NewGuid();
            var testEntity = new ConverterTestEntity { Id = id };
            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);
            var serializationContext = new KiotaJsonSerializationContext(serializerOptions);
            using var jsonSerializerWriter = new JsonSerializationWriter(serializationContext);

            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = $"{{\"id\":\"{id:D}\"}}";
            Assert.Equal(expectedString, serializedJsonString);
        }
        [Fact]
        public void WritesSampleObjectValueWithUntypedProperties()
        {
            // Arrange
            var untypedTestEntity = new UntypedTestEntity
            {
                Id = "1",
                Title = "Title",
                Location = new UntypedObject(new Dictionary<string, UntypedNode>
                    {
                        {"address", new UntypedObject(new Dictionary<string, UntypedNode>
                        {
                            {"city", new UntypedString("Redmond") },
                            {"postalCode", new UntypedString("98052") },
                            {"state", new UntypedString("Washington") },
                            {"street", new UntypedString("NE 36th St") }
                        })},
                        {"coordinates", new UntypedObject(new Dictionary<string, UntypedNode>
                        {
                            {"latitude", new UntypedDouble(47.641942d) },
                            {"longitude", new UntypedDouble(-122.127222d) }
                        })},
                        {"displayName", new UntypedString("Microsoft Building 92") },
                        {"floorCount", new UntypedInteger(50) },
                        {"hasReception", new UntypedBoolean(true) },
                        {"contact", new UntypedNull() }
                    }),
                Keywords = new UntypedArray(new List<UntypedNode>
                {
                    new UntypedObject(new Dictionary<string, UntypedNode>
                    {
                        {"created", new UntypedString("2023-07-26T10:41:26Z") },
                        {"label", new UntypedString("Keyword1") },
                        {"termGuid", new UntypedString("10e9cc83-b5a4-4c8d-8dab-4ada1252dd70") },
                        {"wssId", new UntypedLong(6442450941) }
                    }),
                    new UntypedObject(new Dictionary<string, UntypedNode>
                    {
                        {"created", new UntypedString("2023-07-26T10:51:26Z") },
                        {"label", new UntypedString("Keyword2") },
                        {"termGuid", new UntypedString("2cae6c6a-9bb8-4a78-afff-81b88e735fef") },
                        {"wssId", new UntypedLong(6442450942) }
                    })
                }),
                AdditionalData = new Dictionary<string, object>
                {
                    { "extra", new UntypedObject(new Dictionary<string, UntypedNode>
                    {
                        {"createdDateTime", new UntypedString("2024-01-15T00:00:00+00:00") }
                    }) }
                }
            };
            using var jsonSerializerWriter = new JsonSerializationWriter();
            // Act
            jsonSerializerWriter.WriteObjectValue(string.Empty, untypedTestEntity);
            // Get the json string from the stream.
            var serializedStream = jsonSerializerWriter.GetSerializedContent();
            using var reader = new StreamReader(serializedStream, Encoding.UTF8);
            var serializedJsonString = reader.ReadToEnd();

            // Assert
            var expectedString = "{" +
                "\"id\":\"1\"," +
                "\"title\":\"Title\"," +
                "\"location\":{" +
                "\"address\":{\"city\":\"Redmond\",\"postalCode\":\"98052\",\"state\":\"Washington\",\"street\":\"NE 36th St\"}," +
                "\"coordinates\":{\"latitude\":47.641942,\"longitude\":-122.127222}," +
                "\"displayName\":\"Microsoft Building 92\"," +
                "\"floorCount\":50," +
                "\"hasReception\":true," +
                "\"contact\":null}," +
                "\"keywords\":[" +
                "{\"created\":\"2023-07-26T10:41:26Z\",\"label\":\"Keyword1\",\"termGuid\":\"10e9cc83-b5a4-4c8d-8dab-4ada1252dd70\",\"wssId\":6442450941}," +
                "{\"created\":\"2023-07-26T10:51:26Z\",\"label\":\"Keyword2\",\"termGuid\":\"2cae6c6a-9bb8-4a78-afff-81b88e735fef\",\"wssId\":6442450942}]," +
                "\"extra\":{\"createdDateTime\":\"2024-01-15T00:00:00\\u002B00:00\"}}";
            Assert.Equal(expectedString, serializedJsonString);
        }

        [Fact]
        public void WritesStringValue()
        {
            // Arrange
            var value = "This is a string value";

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteStringValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("\"This is a string value\"", serializedString);
        }

        [Fact]
        public void StreamIsReadableAfterDispose()
        {
            // Arrange
            var value = "This is a string value";

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteStringValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            // Dispose the writer
            jsonSerializationWriter.Dispose();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("\"This is a string value\"", serializedString);
        }

        [Fact]
        public void WriteBoolValue_IsWrittenCorrectly()
        {
            // Arrange
            var value = true;

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteBoolValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("true", serializedString);
        }

        [Fact]
        public void WriteByteArrayValue_IsWrittenCorrectly()
        {
            // Arrange
            var value = new byte[] { 2, 4, 6 };

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteByteArrayValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("\"AgQG\"", serializedString);
        }

        [Fact]
        public void WriteByteValue_IsWrittenCorrectly()
        {
            // Arrange
            byte value = 5;

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteByteValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("5", serializedString);
        }

        [Fact]
        public void WriteDateTimeOffsetValue_IsWrittenCorrectly()
        {
            // Arrange
            var value = new DateTimeOffset(2024, 11, 30, 15, 35, 45, 987, TimeSpan.FromHours(3));

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteDateTimeOffsetValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("\"2024-11-30T15:35:45.987+03:00\"", serializedString);
        }

        [Fact]
        public void WriteDateValue_IsWrittenCorrectly()
        {
            // Arrange
            var value = new Date(2024, 11, 30);

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteDateValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("\"2024-11-30\"", serializedString);
        }

        [Fact]
        public void WriteDecimalValue_IsWrittenCorrectly()
        {
            // Arrange
            var value = 36.8m;

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteDecimalValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
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

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteDoubleValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
#if NET462
            Assert.Equal("36.799999999999997", serializedString);
#else
            Assert.Equal("36.8", serializedString);
#endif
        }

        [Fact]
        public void WriteFloatValue_IsWrittenCorrectly()
        {
            // Arrange
            var value = 36.8f;

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteFloatValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
#if NET462
            Assert.Equal("36.7999992", serializedString);
#else
            Assert.Equal("36.8", serializedString);
#endif
        }

        [Fact]
        public void WriteGuidValue_IsWrittenCorrectly()
        {
            // Arrange
            var value = new Guid("3adeb301-58f1-45c5-b820-ae5f4af13c89");

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteGuidValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("\"3adeb301-58f1-45c5-b820-ae5f4af13c89\"", serializedString);
        }

        [Fact]
        public void WriteIntegerValue_IsWrittenCorrectly()
        {
            // Arrange
            var value = 25;

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteIntValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
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

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteLongValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("9223372036854775807", serializedString);
        }

        [Fact]
        public void WriteNullValue_IsWrittenCorrectly()
        {
            // Arrange
            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteNullValue(null);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
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

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteSbyteValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
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

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteTimeValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("\"23:46:59\"", serializedString);
        }

        [Fact]
        public void WriteTimeSpanValue_IsWrittenCorrectly()
        {
            // Arrange
            var value = new TimeSpan(756, 4, 6, 8, 10);

            using var jsonSerializationWriter = new JsonSerializationWriter();

            // Act
            jsonSerializationWriter.WriteTimeSpanValue(null, value);
            var contentStream = jsonSerializationWriter.GetSerializedContent();
            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var serializedString = reader.ReadToEnd();

            // Assert
            Assert.Equal("\"P756DT4H6M8.01S\"", serializedString);
        }
    }
}
