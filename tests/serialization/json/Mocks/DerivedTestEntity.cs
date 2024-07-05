using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Json.Tests.Mocks
{
    public class DerivedTestEntity : TestEntity
    {
        /// <summary>
        /// Date enrolled in primary school
        /// </summary>
        public Date? EnrolmentDate { get; set; }
        public override IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            var parentDeserializers = base.GetFieldDeserializers();
            parentDeserializers.Add("enrolmentDate", n => { EnrolmentDate = n.GetDateValue(); });
            return parentDeserializers;
        }
        public override void Serialize(ISerializationWriter writer)
        {
            base.Serialize(writer);
            writer.WriteDateValue("enrolmentDate", EnrolmentDate.Value);
        }
    }
}