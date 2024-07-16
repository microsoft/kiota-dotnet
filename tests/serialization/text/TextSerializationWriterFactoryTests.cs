using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Text.Tests
{
    public class TextSerializationWriterFactoryTests
    {
        [Fact]
        public void TextSerializationWriterFactory_GetSerializationWriter()
        {
            var factory = new TextSerializationWriterFactory();
            var writer = factory.GetSerializationWriter("text/plain");
            Assert.NotNull(writer);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void TextSerializationWriterFactory_GetSerializationWriter_ThrowsOnNullContentType(string? contentType)
        {
            var factory = new TextSerializationWriterFactory();
            Assert.Throws<ArgumentNullException>(() => factory.GetSerializationWriter(contentType!));
        }

        [Fact]
        public void TextSerializationWriterFactory_GetSerializationWriter_ThrowsOnInvalidContentType()
        {
            var factory = new TextSerializationWriterFactory();
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.GetSerializationWriter("application/json"));
        }
    }
}
