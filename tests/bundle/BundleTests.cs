using Xunit;
using Moq;
using System;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Bundle.Tests
{
    public class BundleTests
    {
        [Fact]
        public void ThrowsArgumentNullExceptionOnNullAuthenticationProvider()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new KiotaRequestAdapter(null!));
            Assert.Equal("authenticationProvider", exception.ParamName);
        }

        [Fact]
        public void SerializersAreRegisteredAsExpected()
        {
            // setup
            KiotaRequestAdapter requestAdapter = new KiotaRequestAdapter(new AnonymousAuthenticationProvider());

            // validate
            var serializerCount = SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Count;
            var deserializerCount = ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Count;

            Assert.Equal(4, serializerCount); // four serializers present
            Assert.Equal(3, deserializerCount);// three deserializers present

            var serializerKeys = SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Keys;
            var deserializerKeys = ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Keys;

            Assert.Contains("application/json", serializerKeys);
            Assert.Contains("application/json", deserializerKeys);// Serializer and deserializer present for application/json

            Assert.Contains("text/plain", serializerKeys);
            Assert.Contains("text/plain", deserializerKeys);// Serializer and deserializer present for text/plain

            Assert.Contains("application/x-www-form-urlencoded", serializerKeys);
            Assert.Contains("application/x-www-form-urlencoded", deserializerKeys);// Serializer and deserializer present for application/x-www-form-urlencoded

            Assert.Contains("multipart/form-data", serializerKeys);// Serializer present for multipart/form-data
        }
    }
}
