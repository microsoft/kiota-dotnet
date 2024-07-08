
using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Multipart.Tests.Mocks;

public class SecondTestEntity : IParsable
{
    public string DisplayName { get; set; }
    public int? Id { get; set; } // intentionally conflicts with the Id property of the TestEntity class
    public long? FailureRate { get; set; }
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        return new Dictionary<string, Action<IParseNode>> {
            { "displayName", node => DisplayName = node.GetStringValue() },
            { "id", node => Id = node.GetIntValue() },
            { "failureRate", node => FailureRate = node.GetLongValue()},
        };
    }
    public void Serialize(ISerializationWriter writer)
    {
        writer.WriteStringValue("displayName", DisplayName);
        writer.WriteIntValue("id", Id);
        writer.WriteLongValue("failureRate", FailureRate);
    }
}