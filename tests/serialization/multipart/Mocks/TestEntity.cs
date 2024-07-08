using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Microsoft.Kiota.Serialization.Multipart.Tests.Mocks
{
    public class TestEntity : IParsable, IAdditionalDataHolder
    {
        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData { get; set; }
        /// <summary>Read-only.</summary>
        public string Id { get; set; }
        /// <summary>Read-only.</summary>
        public TestEnum? Numbers { get; set; }
        /// <summary>Read-only.</summary>
        public TestNamingEnum? TestNamingEnum { get; set; }
        /// <summary>Read-only.</summary>
        public TimeSpan? WorkDuration { get; set; }
        /// <summary>Read-only.</summary>
        public Date? BirthDay { get; set; }
        /// <summary>Read-only.</summary>
        public Time? StartWorkTime { get; set; }
        /// <summary>Read-only.</summary>
        public Time? EndWorkTime { get; set; }
        /// <summary>Read-only.</summary>
        public DateTimeOffset? CreatedDateTime { get; set; }
        /// <summary>Read-only.</summary>
        public string OfficeLocation { get; set; }
        /// <summary>
        /// Instantiates a new entity and sets the default values.
        /// </summary>
        public TestEntity()
        {
            AdditionalData = new Dictionary<string, object>();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>> {
                {"id", n => { Id = n.GetStringValue(); } },
                {"numbers", n => { Numbers = n.GetEnumValue<TestEnum>(); } },
                {"testNamingEnum", n => { TestNamingEnum = n.GetEnumValue<TestNamingEnum>(); } },
                {"createdDateTime", n => { CreatedDateTime = n.GetDateTimeOffsetValue(); } },
                {"officeLocation", n => { OfficeLocation = n.GetStringValue(); } },
                {"workDuration", n => { WorkDuration = n.GetTimeSpanValue(); } },
                {"birthDay", n => { BirthDay = n.GetDateValue(); } },
                {"startWorkTime", n => { StartWorkTime = n.GetTimeValue(); } },
                {"endWorkTime", n => { EndWorkTime = n.GetTimeValue(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        /// </summary>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("id", Id);
            writer.WriteEnumValue<TestEnum>("numbers", Numbers);
            writer.WriteEnumValue<TestNamingEnum>("testNamingEnum", TestNamingEnum);
            writer.WriteDateTimeOffsetValue("createdDateTime", CreatedDateTime);
            writer.WriteStringValue("officeLocation", OfficeLocation);
            writer.WriteTimeSpanValue("workDuration", WorkDuration);
            writer.WriteDateValue("birthDay", BirthDay);
            writer.WriteTimeValue("startWorkTime", StartWorkTime);
            writer.WriteTimeValue("endWorkTime", EndWorkTime);
            writer.WriteAdditionalData(AdditionalData);
        }
        public static TestEntity CreateFromDiscriminator(IParseNode parseNode)
        {
            return new TestEntity();
        }
    }
}
