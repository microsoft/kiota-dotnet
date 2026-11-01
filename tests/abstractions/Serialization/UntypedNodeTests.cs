using System.Collections.Generic;
using Microsoft.Kiota.Abstractions.Serialization;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests.Serialization;

public class UntypedNodeTests
{
    [Fact]
    public void UntypedNodePolymorphicGetValue()
    {
        UntypedNode strNode = new UntypedString("hello");
        Assert.Equal("hello", strNode.GetValue());

        UntypedNode boolNode = new UntypedBoolean(true);
        Assert.Equal(true, boolNode.GetValue());

        UntypedNode intNode = new UntypedInteger(42);
        Assert.Equal(42, intNode.GetValue());

        UntypedNode longNode = new UntypedLong(123456789L);
        Assert.Equal(123456789L, longNode.GetValue());

        UntypedNode floatNode = new UntypedFloat(3.14f);
        Assert.Equal(3.14f, floatNode.GetValue());

        UntypedNode doubleNode = new UntypedDouble(2.71828);
        Assert.Equal(2.71828, doubleNode.GetValue());

        UntypedNode decimalNode = new UntypedDecimal(99.99m);
        Assert.Equal(99.99m, decimalNode.GetValue());

        UntypedNode nullNode = new UntypedNull();
        Assert.Null(nullNode.GetValue());

        var properties = new Dictionary<string, UntypedNode>
        {
            { "key", new UntypedString("val") }
        };
        UntypedNode objNode = new UntypedObject(properties);
        Assert.Same(properties, objNode.GetValue());

        var items = new List<UntypedNode> { new UntypedInteger(1) };
        UntypedNode arrayNode = new UntypedArray(items);
        Assert.Same(items, arrayNode.GetValue());

        UntypedNode baseNode = new UntypedNode();
        Assert.Null(baseNode.GetValue());
    }
}
