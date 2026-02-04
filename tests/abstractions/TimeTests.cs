using System;
using System.Text.Json;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests
{
    public class TimeTests
    {
        [Fact]
        public void TestTimeEquality()
        {
            var time1 = new Time(10, 30, 0);
            var time2 = new Time(new DateTime(2024, 7, 17, 10, 30, 0));
            var time3 = new Time(12, 0, 0);

            Assert.Equal(time1, time2);
            Assert.NotEqual(time1, time3);
        }

        [Fact]
        public void TestTimeToString()
        {
            var time = new Time(15, 45, 30);
            var expectedString = "15:45:30";

            Assert.Equal(expectedString, time.ToString());
        }

        [Fact]
        public void TestTimeSerialization()
        {
            var time = new Time(10, 18, 54);
            var serialized = JsonSerializer.Serialize(time);

            // System.Text.Json should serialize the Time object with its properties
            Assert.Contains("\"Hour\":10", serialized);
            Assert.Contains("\"Minute\":18", serialized);
            Assert.Contains("\"Second\":54", serialized);
        }

        [Fact]
        public void TestTimeDeserialization()
        {
            // This is the issue scenario - deserializing a Time object from JSON
            var json = "{\"DateTime\":\"2025-10-24T10:18:54.5090402-05:00\",\"Hour\":10,\"Minute\":18,\"Second\":54}";
            var time = JsonSerializer.Deserialize<Time>(json);

            Assert.Equal(10, time.Hour);
            Assert.Equal(18, time.Minute);
            Assert.Equal(54, time.Second);
        }

        [Fact]
        public void TestTimeRoundTrip()
        {
            // Test that we can serialize and deserialize a Time object
            var original = new Time(10, 18, 54);
            var serialized = JsonSerializer.Serialize(original);
            var deserialized = JsonSerializer.Deserialize<Time>(serialized);

            Assert.Equal(original.Hour, deserialized.Hour);
            Assert.Equal(original.Minute, deserialized.Minute);
            Assert.Equal(original.Second, deserialized.Second);
        }
    }
}
