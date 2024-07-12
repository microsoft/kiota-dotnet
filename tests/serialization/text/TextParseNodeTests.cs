using Microsoft.Kiota.Serialization.Text.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Serialization.Text.Tests
{
    public class TextParseNodeTests
    {
        [Fact]
        public void TextParseNode_GetEnumFromInteger()
        {
            var text = "1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetEnumValue<TestEnum>();

            Assert.Equal(TestEnum.Second, result);
        }

        [Fact]
        public void TextParseNode_GetEnumFromString()
        {
            var text = "First";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetEnumValue<TestEnum>();

            Assert.Equal(TestEnum.First, result);
        }

        [Fact]
        public void TextParseNode_GetEnumFromEnumMember()
        {
            var text = "Value_2";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetEnumValue<TestEnum>();

            Assert.Equal(TestEnum.Second, result);
        }
    }
}
