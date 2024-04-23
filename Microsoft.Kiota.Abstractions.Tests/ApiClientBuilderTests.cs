using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests
{
    public class ApiClientBuilderTests
    {
        private const string StreamContentType = "application/octet-stream";

        [Fact]
        public void EnableBackingStoreForSerializationWriterFactory()
        {
            // Arrange
            var serializationFactoryRegistry = new SerializationWriterFactoryRegistry();
            var mockSerializationWriterFactory = new Mock<ISerializationWriterFactory>();
            serializationFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockSerializationWriterFactory.Object);

            Assert.IsNotType<BackingStoreSerializationWriterProxyFactory>(serializationFactoryRegistry.ContentTypeAssociatedFactories[StreamContentType]);

            // Act
            ApiClientBuilder.EnableBackingStoreForSerializationWriterFactory(serializationFactoryRegistry);

            // Assert the type has changed due to backing store enabling
            Assert.IsType<BackingStoreSerializationWriterProxyFactory>(serializationFactoryRegistry.ContentTypeAssociatedFactories[StreamContentType]);
        }

        [Fact]
        public void EnableBackingStoreForSerializationWriterFactoryAlsoEnablesForDefaultInstance()
        {
            // Arrange
            var serializationFactoryRegistry = new SerializationWriterFactoryRegistry();
            var mockSerializationWriterFactory = new Mock<ISerializationWriterFactory>();
            serializationFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockSerializationWriterFactory.Object);
            SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockSerializationWriterFactory.Object);

            Assert.IsNotType<BackingStoreSerializationWriterProxyFactory>(serializationFactoryRegistry.ContentTypeAssociatedFactories[StreamContentType]);

            // Act
            ApiClientBuilder.EnableBackingStoreForSerializationWriterFactory(serializationFactoryRegistry);

            // Assert the type has changed due to backing store enabling for the default instance as well.
            Assert.IsType<BackingStoreSerializationWriterProxyFactory>(serializationFactoryRegistry.ContentTypeAssociatedFactories[StreamContentType]);
            Assert.IsType<BackingStoreSerializationWriterProxyFactory>(SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[StreamContentType]);
        }

        [Fact]
        public void EnableBackingStoreForParseNodeFactory()
        {
            // Arrange
            var parseNodeRegistry = new ParseNodeFactoryRegistry();
            var mockParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
            parseNodeRegistry.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockParseNodeFactory.Object);

            Assert.IsNotType<BackingStoreParseNodeFactory>(parseNodeRegistry.ContentTypeAssociatedFactories[StreamContentType]);

            // Act
            ApiClientBuilder.EnableBackingStoreForParseNodeFactory(parseNodeRegistry);

            // Assert the type has changed due to backing store enabling
            Assert.IsType<BackingStoreParseNodeFactory>(parseNodeRegistry.ContentTypeAssociatedFactories[StreamContentType]);
        }

        [Fact]
        public void EnableBackingStoreForParseNodeFactoryAlsoEnablesForDefaultInstance()
        {
            // Arrange
            var parseNodeRegistry = new ParseNodeFactoryRegistry();
            var mockParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
            parseNodeRegistry.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockParseNodeFactory.Object);
            ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockParseNodeFactory.Object);

            Assert.IsNotType<BackingStoreParseNodeFactory>(parseNodeRegistry.ContentTypeAssociatedFactories[StreamContentType]);

            // Act
            ApiClientBuilder.EnableBackingStoreForParseNodeFactory(parseNodeRegistry);

            // Assert the type has changed due to backing store enabling for the default instance as well.
            Assert.IsType<BackingStoreParseNodeFactory>(parseNodeRegistry.ContentTypeAssociatedFactories[StreamContentType]);
            Assert.IsType<BackingStoreParseNodeFactory>(ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[StreamContentType]);
        }
    }
}
