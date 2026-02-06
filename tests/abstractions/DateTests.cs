using System;
using System.Text.Json;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests
{
    public class DateTests
    {
        [Fact]
        public void TestDateSerialization()
        {
            var date = new Date(2025, 10, 24);
            var serialized = JsonSerializer.Serialize(date);

            // System.Text.Json should serialize the Date object with its properties
            Assert.Contains("\"Year\":2025", serialized);
            Assert.Contains("\"Month\":10", serialized);
            Assert.Contains("\"Day\":24", serialized);
        }

        [Fact]
        public void TestDateDeserialization()
        {
            // This is the issue scenario - deserializing a Date object from JSON
            var json = "{\"DateTime\":\"2025-10-24T10:18:54.5003283-05:00\",\"Year\":2025,\"Month\":10,\"Day\":24}";
            var date = JsonSerializer.Deserialize<Date>(json);

            Assert.Equal(2025, date.Year);
            Assert.Equal(10, date.Month);
            Assert.Equal(24, date.Day);
        }

        [Fact]
        public void TestDateRoundTrip()
        {
            // Test that we can serialize and deserialize a Date object
            var original = new Date(2025, 10, 24);
            var serialized = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<Date>(serialized);

            Assert.Equal(original.Year, deserialized.Year);
            Assert.Equal(original.Month, deserialized.Month);
            Assert.Equal(original.Day, deserialized.Day);
        }

        [Fact]
        public void TestDateToString()
        {
            var date = new Date(2025, 10, 24);
            var expectedString = "2025-10-24";

            Assert.Equal(expectedString, date.ToString());
        }
    }
}
