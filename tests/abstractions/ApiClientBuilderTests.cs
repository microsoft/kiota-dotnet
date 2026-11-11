using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task EnableBackingStoreForSerializationWriterFactoryConcurrentlyDoesNotDoubleWrap()
        {
            var proxiedFieldInfo = typeof(SerializationWriterProxyFactory)
                .GetField("ProxiedSerializationWriterFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            for(var round = 0; round < 50; round++)
            {
                var registry = new SerializationWriterFactoryRegistry();
                var mockWriterFactory = new Mock<ISerializationWriterFactory>();
                registry.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockWriterFactory.Object);

                using var start = new ManualResetEventSlim();
                var workers = Enumerable.Range(0, 8)
                    .Select(_ => Task.Factory.StartNew(() =>
                    {
                        start.Wait();
                        ApiClientBuilder.EnableBackingStoreForSerializationWriterFactory(registry);
                    }, TaskCreationOptions.LongRunning))
                    .ToArray();

                start.Set();
                await Task.WhenAll(workers);

                var factory = registry.ContentTypeAssociatedFactories[StreamContentType];
                Assert.IsType<BackingStoreSerializationWriterProxyFactory>(factory);
                Assert.Equal(mockWriterFactory.Object, proxiedFieldInfo!.GetValue(factory));
            }
        }

        [Fact]
        public void EnableBackingStoreForParseNodeFactory()
        {
            // Arrange
            var parseNodeRegistry = new ParseNodeFactoryRegistry();
            var mockParseNodeFactory = new Mock<IParseNodeFactory>();
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
            var mockParseNodeFactory = new Mock<IParseNodeFactory>();
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
            var mockParseNodeFactory = new Mock<IParseNodeFactory>();
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
            var mockParseNodeFactory = new Mock<IParseNodeFactory>();
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

        [Fact]
        public async Task EnableBackingStoreForParseNodeFactoryConcurrentlyDoesNotDoubleWrap()
        {
            var concreteFieldInfo = typeof(ParseNodeProxyFactory)
                .GetField("_concrete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            for(var round = 0; round < 50; round++)
            {
                var registry = new ParseNodeFactoryRegistry();
                var mockParseNodeFactory = new Mock<IParseNodeFactory>();
                registry.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockParseNodeFactory.Object);

                using var start = new ManualResetEventSlim();
                var workers = Enumerable.Range(0, 8)
                    .Select(_ => Task.Factory.StartNew(() =>
                    {
                        start.Wait();
                        ApiClientBuilder.EnableBackingStoreForParseNodeFactory(registry);
                    }, TaskCreationOptions.LongRunning))
                    .ToArray();

                start.Set();
                await Task.WhenAll(workers);

                var factory = registry.ContentTypeAssociatedFactories[StreamContentType];
                Assert.IsType<BackingStoreParseNodeFactory>(factory);
                Assert.Equal(mockParseNodeFactory.Object, concreteFieldInfo!.GetValue(factory));
            }
        }

        [Fact]
        public async Task ConcurrentEnableBackingStoreWritesNullPropertyOnlyOnce()
        {
            var mockWriter = new Mock<ISerializationWriter>();
            mockWriter.SetupAllProperties();
            var mockWriterFactory = new Mock<ISerializationWriterFactory>();
            mockWriterFactory.Setup(x => x.GetSerializationWriter(StreamContentType)).Returns(mockWriter.Object);

            var registry = new SerializationWriterFactoryRegistry();
            registry.ContentTypeAssociatedFactories.TryAdd(StreamContentType, mockWriterFactory.Object);

            using var start = new ManualResetEventSlim();
            var workers = Enumerable.Range(0, 8)
                .Select(_ => Task.Factory.StartNew(() =>
                {
                    start.Wait();
                    ApiClientBuilder.EnableBackingStoreForSerializationWriterFactory(registry);
                }, TaskCreationOptions.LongRunning))
                .ToArray();

            start.Set();
            await Task.WhenAll(workers);

            var backingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
            backingStore.InitializationCompleted = true;
            backingStore.Set("city", (string?)null);

            var mockBackedModel = new Mock<IBackedModel>();
            mockBackedModel.As<IParsable>();
            mockBackedModel.SetupGet(m => m.BackingStore).Returns(backingStore);

            using var writer = registry.GetSerializationWriter(StreamContentType);
            writer.OnStartObjectSerialization?.Invoke((IParsable)mockBackedModel.Object, mockWriter.Object);

            mockWriter.Verify(w => w.WriteNullValue("city"), Times.Once());
        }
    }
}
