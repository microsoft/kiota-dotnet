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
        public void EnableBackingStoreForParseNodeFactoryMultipleCallsDoesNotDoubleWrap()
        {
            // Arrange
            //it is not normal to test a private field, but it is the purpose of the test
            var concreteFieldInfo = typeof(ParseNodeProxyFactory)
                                        .GetField("_concrete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);


            var parseNodeRegistry = new ParseNodeFactoryRegistry();
            var mockParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
            parseNodeRegistry.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockParseNodeFactory.Object);

            Assert.IsNotType<BackingStoreParseNodeFactory>(parseNodeRegistry.ContentTypeAssociatedFactories[StreamContentType]);

            // Act
            var firstResult = ApiClientBuilder.EnableBackingStoreForParseNodeFactory(parseNodeRegistry);
            var secondResult = ApiClientBuilder.EnableBackingStoreForParseNodeFactory(firstResult);
            var thirdResult = ApiClientBuilder.EnableBackingStoreForParseNodeFactory(secondResult);


            //make sure the original was not modified
            Assert.IsNotType<BackingStoreParseNodeFactory>(parseNodeRegistry);

            // Assert the type has NOT changed for registries to avoid double wrapping
            Assert.IsType<ParseNodeFactoryRegistry>(firstResult);

            //make sure the second call returned the original wrapper
            Assert.Equal(firstResult, secondResult);
            Assert.Equal(firstResult, thirdResult);

            //make sure what is in the registry is a BackingStore
            var factory = parseNodeRegistry.ContentTypeAssociatedFactories[StreamContentType];
            Assert.IsType<BackingStoreParseNodeFactory>(factory);

            //make sure the concrete version of the factory is the same as the orginal
            Assert.Equal(mockParseNodeFactory.Object, concreteFieldInfo!.GetValue(factory));
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

        [Fact]
        public void EnableBackingStoreForParseNodeFactoryAlsoEnablesForDefaultInstanceMultipleCallsDoesNotDoubleWrap()
        {
            // Arrange
            var parseNodeRegistry = new ParseNodeFactoryRegistry();
            var mockParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
            parseNodeRegistry.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockParseNodeFactory.Object);
            ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockParseNodeFactory.Object);

            Assert.IsNotType<BackingStoreParseNodeFactory>(parseNodeRegistry.ContentTypeAssociatedFactories[StreamContentType]);

            // Act
            var firstResult = ApiClientBuilder.EnableBackingStoreForParseNodeFactory(parseNodeRegistry);
            var secondResult = ApiClientBuilder.EnableBackingStoreForParseNodeFactory(firstResult);
            var thirdResult = ApiClientBuilder.EnableBackingStoreForParseNodeFactory(secondResult);


            //make sure the original was not modified
            Assert.IsNotType<BackingStoreParseNodeFactory>(parseNodeRegistry);

            // Assert the type has NOT changed for registries to avoid double wrapping
            Assert.IsType<ParseNodeFactoryRegistry>(firstResult);

            //make sure the second call returned the original wrapper
            Assert.Equal(firstResult, secondResult);
            Assert.Equal(firstResult, thirdResult);

            //make sure what is in the registry is a BackingStore, it will be a new object so we can only check the type
            var factory = ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[StreamContentType];
            Assert.IsType<BackingStoreParseNodeFactory>(factory);

        }
    }
}
