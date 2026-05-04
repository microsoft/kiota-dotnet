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
    public class ParseNodeProxyFactoryTests
    {
        private readonly Mock<IParseNodeFactory> _mockConcreteFactory;
        private readonly Mock<IParseNode> _mockParseNode;

        public ParseNodeProxyFactoryTests()
        {
            _mockConcreteFactory = new Mock<IParseNodeFactory>();
            _mockParseNode = new Mock<IParseNode>();
            _mockConcreteFactory
                .Setup(f => f.ValidContentType)
                .Returns("application/json");
        }

        [Fact]
        public void ValidContentType_ReturnsConceteFactoryContentType()
        {
            // Arrange
            var proxyFactory = new TestParseNodeProxyFactory(_mockConcreteFactory.Object, null, null);
            // Act & Assert
            Assert.Equal("application/json", proxyFactory.ValidContentType);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenConcreteIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TestParseNodeProxyFactory(null!, null, null));
        }

        [Fact]
        public async Task GetRootParseNodeAsync_ReturnsNodeFromConcreteFactory()
        {
            // Arrange
            _mockConcreteFactory
                .Setup(f => f.GetRootParseNodeAsync("application/json", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockParseNode.Object);
            var proxyFactory = new TestParseNodeProxyFactory(_mockConcreteFactory.Object, null, null);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"test\": true}"));

            // Act
            var result = await proxyFactory.GetRootParseNodeAsync("application/json", stream, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(_mockParseNode.Object, result);
        }

        [Fact]
        public async Task GetRootParseNodeAsync_InvokesOnBeforeCallback()
        {
            // Arrange
            var onBeforeCalled = false;
            _mockParseNode.SetupProperty(n => n.OnBeforeAssignFieldValues);
            _mockParseNode.SetupProperty(n => n.OnAfterAssignFieldValues);
            _mockConcreteFactory
                .Setup(f => f.GetRootParseNodeAsync("application/json", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockParseNode.Object);
            var proxyFactory = new TestParseNodeProxyFactory(
                _mockConcreteFactory.Object,
                onBefore: _ => onBeforeCalled = true,
                onAfter: null);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

            // Act
            var node = await proxyFactory.GetRootParseNodeAsync("application/json", stream, TestContext.Current.CancellationToken);
            node.OnBeforeAssignFieldValues?.Invoke(Mock.Of<IParsable>());

            // Assert
            Assert.True(onBeforeCalled);
        }

        [Fact]
        public async Task GetRootParseNodeAsync_InvokesOnAfterCallback()
        {
            // Arrange
            var onAfterCalled = false;
            _mockParseNode.SetupProperty(n => n.OnBeforeAssignFieldValues);
            _mockParseNode.SetupProperty(n => n.OnAfterAssignFieldValues);
            _mockConcreteFactory
                .Setup(f => f.GetRootParseNodeAsync("application/json", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockParseNode.Object);
            var proxyFactory = new TestParseNodeProxyFactory(
                _mockConcreteFactory.Object,
                onBefore: null,
                onAfter: _ => onAfterCalled = true);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

            // Act
            var node = await proxyFactory.GetRootParseNodeAsync("application/json", stream, TestContext.Current.CancellationToken);
            node.OnAfterAssignFieldValues?.Invoke(Mock.Of<IParsable>());

            // Assert
            Assert.True(onAfterCalled);
        }

        [Fact]
        public async Task GetRootParseNodeAsync_PreservesOriginalCallbacks()
        {
            // Arrange
            var originalBeforeCalled = false;
            var originalAfterCalled = false;
            var proxyBeforeCalled = false;
            var proxyAfterCalled = false;

            _mockParseNode.SetupProperty(n => n.OnBeforeAssignFieldValues, (Action<IParsable>)(_ => originalBeforeCalled = true));
            _mockParseNode.SetupProperty(n => n.OnAfterAssignFieldValues, (Action<IParsable>)(_ => originalAfterCalled = true));

            _mockConcreteFactory
                .Setup(f => f.GetRootParseNodeAsync("application/json", It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockParseNode.Object);

            var proxyFactory = new TestParseNodeProxyFactory(
                _mockConcreteFactory.Object,
                onBefore: _ => proxyBeforeCalled = true,
                onAfter: _ => proxyAfterCalled = true);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

            // Act
            var node = await proxyFactory.GetRootParseNodeAsync("application/json", stream, TestContext.Current.CancellationToken);
            node.OnBeforeAssignFieldValues?.Invoke(Mock.Of<IParsable>());
            node.OnAfterAssignFieldValues?.Invoke(Mock.Of<IParsable>());

            // Assert
            Assert.True(originalBeforeCalled);
            Assert.True(originalAfterCalled);
            Assert.True(proxyBeforeCalled);
            Assert.True(proxyAfterCalled);
        }

        /// <summary>
        /// Concrete test implementation of the abstract ParseNodeProxyFactory.
        /// </summary>
        private class TestParseNodeProxyFactory : ParseNodeProxyFactory
        {
            public TestParseNodeProxyFactory(IParseNodeFactory concrete, Action<IParsable>? onBefore, Action<IParsable>? onAfter)
                : base(concrete, onBefore!, onAfter!)
            {
            }
        }
    }
}
