using System;
using System.Globalization;
using System.Threading;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Text.Tests.Mocks;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Serialization.Text.Tests
{
    public class TextParseNodeTests
    {
        public TextParseNodeTests()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
        }

        [Fact]
        public void TextParseNode_GetEnumFromInteger()
        {
            var text = "1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetEnumValue<TestEnum>();

            Assert.Equal(TestEnum.SecondItem, result);
        }

        [Fact]
        public void TextParseNode_GetEnumFromString()
        {
            var text = "FirstItem";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetEnumValue<TestEnum>();

            Assert.Equal(TestEnum.FirstItem, result);
        }

        [Fact]
        public void TextParseNode_GetEnumFromEnumMember()
        {
            var text = "Item2:SubItem1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetEnumValue<TestNamingEnum>();

            Assert.Equal(TestNamingEnum.Item2SubItem1, result);
        }

        [Fact]
        public void TextParseNode_GetBoolValue()
        {
            var text = "true";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetBoolValue();

            Assert.True(result);
        }

        [Fact]
        public void TextParseNode_GetByteArrayValue()
        {
            var text = "dGV4dA==";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetByteArrayValue();

            Assert.Equal(new byte[] { 0x74, 0x65, 0x78, 0x74 }, result);
        }

        [Theory]
        [InlineData("not-a-valid-base64-string!")]
        [InlineData("???")]
        [InlineData("abc")]
        public void TextParseNode_GetByteArrayValue_ReturnsNullOnInvalidBase64(string invalidBase64)
        {
            var parseNode = new TextParseNode(invalidBase64);

            var result = parseNode.GetByteArrayValue();

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TextParseNode_GetByteArrayValue_ReturnsNullOnNullOrEmpty(string? emptyInput)
        {
            var parseNode = new TextParseNode(emptyInput);

            var result = parseNode.GetByteArrayValue();

            Assert.Null(result);
        }

        [Fact]
        public void TextParseNode_GetByteValue()
        {
            var text = "1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetByteValue();

            Assert.Equal((byte)1, result);
        }

        [Fact]
        public void TextParseNode_GetDateTimeOffsetValue()
        {
            var text = "2021-11-30T12:24:36+03:00";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetDateTimeOffsetValue();

            Assert.Equal(new DateTimeOffset(2021, 11, 30, 12, 24, 36, TimeSpan.FromHours(3)), result);
        }

        [Fact]
        public void TextParseNode_GetDateValue()
        {
            var text = "2021-11-30";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetDateValue();

            Assert.Equal(new Date(2021, 11, 30), result);
        }

        [Fact]
        public void TextParseNode_GetTimeSpanValue()
        {
            var text = "P756DT4H6M8.01S";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetTimeSpanValue();

            Assert.Equal(new TimeSpan(756, 4, 6, 8, 10), result);
        }

        [Theory]
        [InlineData("01:02:03", 0, 1, 2, 3, 0)]
        [InlineData("1.02:03:04", 1, 2, 3, 4, 0)]
        [InlineData("00:00:00.5000000", 0, 0, 0, 0, 500)]
        public void TextParseNode_GetTimeSpanValue_ParsesConstantTimeSpanFormat(string text, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetTimeSpanValue();

            Assert.Equal(new TimeSpan(days, hours, minutes, seconds, milliseconds), result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("null")]
        [InlineData("NULL")]
        [InlineData("not-a-timespan")]
        [InlineData("P10675200D")]
        [InlineData("PT100000000000000000000H")]
        public void TextParseNode_GetTimeSpanValue_ReturnsNullOnInvalidOrOverflowDuration(string? invalidDuration)
        {
            var parseNode = new TextParseNode(invalidDuration);

            var result = parseNode.GetTimeSpanValue();

            Assert.Null(result);
        }

        [Fact]
        public void TextParseNode_GetTimeValue()
        {
            var text = "12:24:36";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetTimeValue();

            Assert.Equal(new Time(12, 24, 36), result);
        }

        [Fact]
        public void TextParseNode_GetGuidValue()
        {
            var text = "f4b3b8f4-6f4d-4f1f-8f4d-8f4b3b8f4d1f";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetGuidValue();

            Assert.Equal(new Guid("f4b3b8f4-6f4d-4f1f-8f4d-8f4b3b8f4d1f"), result);
        }

        [Fact]
        public void TextParseNode_GetIntValue()
        {
            var text = "1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetIntValue();

            Assert.Equal(1, result);
        }

        [Fact]
        public void TextParseNode_GetLongValue()
        {
            var text = "1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetLongValue();

            Assert.Equal(1L, result);
        }

        [Fact]
        public void TextParseNode_GetSbyteValue()
        {
            var text = "1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetSbyteValue();

            Assert.Equal((sbyte)1, result);
        }

        [Fact]
        public void TextParseNode_GetStringValue()
        {
            var text = "text";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetStringValue();

            Assert.Equal("text", result);
        }

        [Fact]
        public void TextParseNode_GetDecimalValue()
        {
            var text = "1.1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetDecimalValue();

            Assert.Equal(1.1m, result);
        }

        [Fact]
        public void TextParseNode_GetDoubleValue()
        {
            var text = "1.1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetDoubleValue();

            Assert.Equal(1.1, result);
        }

        [Fact]
        public void TextParseNode_GetFloatValue()
        {
            var text = "1.1";
            var parseNode = new TextParseNode(text);

            var result = parseNode.GetFloatValue();

            Assert.Equal(1.1f, result);
        }

        [Fact]
        public void TextParseNode_GetCollectionOfPrimitiveValues()
        {
            var text = "1,2,3";
            var parseNode = new TextParseNode(text);

            Assert.Throws<InvalidOperationException>(parseNode.GetCollectionOfPrimitiveValues<int>);
        }

        [Fact]
        public void TextParseNode_GetCollectionOfObjectValues()
        {
            var text = "xxx";
            var parseNode = new TextParseNode(text);

            Assert.Throws<InvalidOperationException>(() => parseNode.GetCollectionOfObjectValues(It.IsAny<ParsableFactory<TestEntity>>()));
        }

        [Fact]
        public void TextParseNode_GetCollectionOfEnumValues()
        {
            var text = "xxx";
            var parseNode = new TextParseNode(text);

            Assert.Throws<InvalidOperationException>(parseNode.GetCollectionOfEnumValues<TestEnum>);
        }
    }
}
