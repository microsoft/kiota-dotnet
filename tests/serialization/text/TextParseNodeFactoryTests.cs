using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Text.Tests
{
    public class TextParseNodeFactoryTests
    {
        [Fact]
        public void GetRootParseNode_ThrowsArgumentNullException_WhenContentTypeIsNull()
        {
            // Arrange
            var factory = new TextParseNodeFactory();
            string? contentType = null;
            using var content = new MemoryStream();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => factory.GetRootParseNode(contentType!, content));
        }

        [Fact]
        public void GetRootParseNode_ThrowsArgumentOutOfRangeException_WhenContentTypeIsInvalid()
        {
            // Arrange
            var factory = new TextParseNodeFactory();
            string contentType = "application/json";
            using var content = new MemoryStream();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.GetRootParseNode(contentType, content));
        }

        [Fact]
        public void GetRootParseNode_ThrowsArgumentNullException_WhenContentIsNull()
        {
            // Arrange
            var factory = new TextParseNodeFactory();
            string contentType = "text/plain";
            Stream? content = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => factory.GetRootParseNode(contentType, content!));
        }

        [Fact]
        public async Task GetRootParseNodeAsync_ThrowsArgumentNullException_WhenContentTypeIsNull()
        {
            // Arrange
            var factory = new TextParseNodeFactory();
            string? contentType = null;
            using var content = new MemoryStream();

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => factory.GetRootParseNodeAsync(contentType!, content, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task GetRootParseNodeAsync_ThrowsArgumentOutOfRangeException_WhenContentTypeIsInvalid()
        {
            // Arrange
            var factory = new TextParseNodeFactory();
            string contentType = "application/json";
            using var content = new MemoryStream();

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => factory.GetRootParseNodeAsync(contentType, content, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task GetRootParseNodeAsync_ThrowsArgumentNullException_WhenContentIsNull()
        {
            // Arrange
            var factory = new TextParseNodeFactory();
            string contentType = "text/plain";
            Stream? content = null;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => factory.GetRootParseNodeAsync(contentType, content!, TestContext.Current.CancellationToken));
        }
    }
}
