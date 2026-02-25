using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Serialization;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests.Serialization
{
    public class ParseNodeFactoryRegistryTests
    {
        private readonly ParseNodeFactoryRegistry _parseNodeFactoryRegistry;
        public ParseNodeFactoryRegistryTests()
        {
            _parseNodeFactoryRegistry = new ParseNodeFactoryRegistry();
        }

        [Fact]
        public void ParseNodeFactoryRegistryDoesNotStickToOneContentType()
        {
            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => _parseNodeFactoryRegistry.ValidContentType);
        }

        [Fact]
        [Obsolete]
        public void ReturnsExpectedRootNodeForRegisteredContentType()
        {
            // Arrange
            var streamContentType = "application/octet-stream";
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("test input"));
            var mockParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
            var mockParseNode = new Mock<IParseNode>();
            mockParseNodeFactory.Setup(parseNodeFactory => parseNodeFactory.GetRootParseNode(streamContentType, It.IsAny<Stream>())).Returns(mockParseNode.Object);
            _parseNodeFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(streamContentType, mockParseNodeFactory.Object);
            // Act
            var rootParseNode = _parseNodeFactoryRegistry.GetRootParseNode(streamContentType, testStream);
            // Assert
            Assert.NotNull(rootParseNode);
            Assert.Equal(mockParseNode.Object, rootParseNode);
        }
        [Fact]
        [Obsolete]
        public void ReturnsExpectedRootNodeForVendorSpecificContentType()
        {
            // Arrange
            var applicationJsonContentType = "application/json";
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("{\"test\": \"input\"}"));
            var mockParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
            var mockParseNode = new Mock<IParseNode>();
            mockParseNodeFactory.Setup(parseNodeFactory => parseNodeFactory.GetRootParseNode(applicationJsonContentType, It.IsAny<Stream>())).Returns(mockParseNode.Object);
            _parseNodeFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(applicationJsonContentType, mockParseNodeFactory.Object);
            // Act
            var rootParseNode = _parseNodeFactoryRegistry.GetRootParseNode("application/vnd+json", testStream);
            // Assert
            Assert.NotNull(rootParseNode);
            Assert.Equal(mockParseNode.Object, rootParseNode);
        }

        [Fact]
        [Obsolete]
        public void ThrowsInvalidOperationExceptionForUnregisteredContentType()
        {
            // Arrange
            var streamContentType = "application/octet-stream";
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("test input"));
            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => _parseNodeFactoryRegistry.GetRootParseNode(streamContentType, testStream));
            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"Content type {streamContentType} does not have a factory registered to be parsed", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [Obsolete]
        public void ThrowsArgumentNullExceptionForNoContentType(string? contentType)
        {
            // Arrange
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("test input"));
            // Act
            var exception = Assert.Throws<ArgumentNullException>(() => _parseNodeFactoryRegistry.GetRootParseNode(contentType!, testStream));
            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName);
        }

        // *****

        [Fact]
        public async Task ReturnsExpectedRootNodeForRegisteredContentTypeAsync()
        {
            // Arrange
            var streamContentType = "application/octet-stream";
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("test input"));
            var mockParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
            var mockParseNode = new Mock<IParseNode>();
            mockParseNodeFactory.Setup(parseNodeFactory => parseNodeFactory.GetRootParseNodeAsync(streamContentType, It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockParseNode.Object));
            _parseNodeFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(streamContentType, mockParseNodeFactory.Object);
            // Act
            var rootParseNode = await _parseNodeFactoryRegistry.GetRootParseNodeAsync(streamContentType, testStream, TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(rootParseNode);
            Assert.Equal(mockParseNode.Object, rootParseNode);
        }
        [Fact]
        public async Task ReturnsExpectedRootNodeForVendorSpecificContentTypeAsync()
        {
            // Arrange
            var applicationJsonContentType = "application/json";
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("{\"test\": \"input\"}"));
            var mockParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
            var mockParseNode = new Mock<IParseNode>();
            mockParseNodeFactory.Setup(parseNodeFactory => parseNodeFactory.GetRootParseNodeAsync(applicationJsonContentType, It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockParseNode.Object));
            _parseNodeFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(applicationJsonContentType, mockParseNodeFactory.Object);
            // Act
            var rootParseNode = await _parseNodeFactoryRegistry.GetRootParseNodeAsync("application/vnd+json", testStream, TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(rootParseNode);
            Assert.Equal(mockParseNode.Object, rootParseNode);
        }

        [Fact]
        public async Task ThrowsInvalidOperationExceptionForUnregisteredContentTypeAsync()
        {
            // Arrange
            var streamContentType = "application/octet-stream";
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("test input"));
            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _parseNodeFactoryRegistry.GetRootParseNodeAsync(streamContentType, testStream, TestContext.Current.CancellationToken));
            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"Content type {streamContentType} does not have a factory registered to be parsed", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ThrowsArgumentNullExceptionForNoContentTypeAsync(string? contentType)
        {
            // Arrange
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("test input"));
            // Act
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _parseNodeFactoryRegistry.GetRootParseNodeAsync(contentType!, testStream, TestContext.Current.CancellationToken));
            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName);
        }
    }
}
