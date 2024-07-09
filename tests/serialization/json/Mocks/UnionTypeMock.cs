using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Json.Tests.Mocks;

public class UnionTypeMock : IParsable, IComposedTypeWrapper
{
    public TestEntity ComposedType1 { get; set; }
    public SecondTestEntity ComposedType2 { get; set; }
    public string StringValue { get; set; }
    public List<TestEntity> ComposedType3 { get; set; }
    public static UnionTypeMock CreateFromDiscriminator(IParseNode parseNode)
    {
        var result = new UnionTypeMock();
        var discriminator = parseNode.GetChildNode("@odata.type")?.GetStringValue();
        if("#microsoft.graph.testEntity".Equals(discriminator))
        {
            result.ComposedType1 = new();
        }
        else if("#microsoft.graph.secondTestEntity".Equals(discriminator))
        {
            result.ComposedType2 = new();
        }
        else if(parseNode.GetStringValue() is string stringValue)
        {
            result.StringValue = stringValue;
        }
        else if(parseNode.GetCollectionOfObjectValues<TestEntity>(TestEntity.CreateFromDiscriminator) is IEnumerable<TestEntity> values && values.Any())
        {
            result.ComposedType3 = values.ToList();
        }
        return result;
    }
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        if(ComposedType1 != null)
            return ComposedType1.GetFieldDeserializers();
        else if(ComposedType2 != null)
            return ComposedType2.GetFieldDeserializers();
        //composed type3 is omitted on purpose
        return new Dictionary<string, Action<IParseNode>>();
    }
    public void Serialize(ISerializationWriter writer)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        if(ComposedType1 != null)
        {
            writer.WriteObjectValue(null, ComposedType1);
        }
        else if(ComposedType2 != null)
        {
            writer.WriteObjectValue(null, ComposedType2);
        }
        else if(ComposedType3 != null)
        {
            writer.WriteCollectionOfObjectValues(null, ComposedType3);
        }
        else if(!string.IsNullOrEmpty(StringValue))
        {
            writer.WriteStringValue(null, StringValue);
        }
    }
}