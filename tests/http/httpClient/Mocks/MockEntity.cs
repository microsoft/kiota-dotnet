using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;

public class MockEntity : IParsable
{
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        return new Dictionary<string, Action<IParseNode>>();
    }

    public void Serialize(ISerializationWriter writer)
    {

    }
    public static MockEntity Factory (IParseNode parseNode) => new MockEntity();
}
