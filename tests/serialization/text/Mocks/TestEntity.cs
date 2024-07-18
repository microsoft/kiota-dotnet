using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Text.Tests.Mocks
{
    public class TestEntity : IParsable
    {
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => throw new NotImplementedException();
        public void Serialize(ISerializationWriter writer) => throw new NotImplementedException();
    }
}
