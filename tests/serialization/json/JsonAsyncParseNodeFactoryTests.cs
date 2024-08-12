using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Json.Tests
{
    public class JsonAsyncParseNodeFactoryTests
    {
        private readonly JsonParseNodeFactory _jsonParseNodeFactory;
        private const string TestJsonString = "{\"key\":\"value\"}";

        public JsonAsyncParseNodeFactoryTests()
        {
            _jsonParseNodeFactory = new JsonParseNodeFactory();
        }

        [Fact]
        public async Task GetsWriterForJsonContentType()
        {
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestJsonString));
            var jsonWriter = await _jsonParseNodeFactory.GetRootParseNodeAsync(_jsonParseNodeFactory.ValidContentType, jsonStream);

            // Assert
            Assert.NotNull(jsonWriter);
            Assert.IsAssignableFrom<JsonParseNode>(jsonWriter);
        }

        [Fact]
        public async Task ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
        {
            var streamContentType = "application/octet-stream";
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestJsonString));
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _jsonParseNodeFactory.GetRootParseNodeAsync(streamContentType, jsonStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"expected a {_jsonParseNodeFactory.ValidContentType} content type", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ThrowsArgumentNullExceptionForNoContentType(string? contentType)
        {
            using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(TestJsonString));
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _jsonParseNodeFactory.GetRootParseNodeAsync(contentType, jsonStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName);
        }
    }
}
