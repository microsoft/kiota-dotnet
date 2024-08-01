using System.Runtime.Serialization;

namespace Microsoft.Kiota.Serialization.Form.Tests.Mocks
{
    public enum TestNamingEnum
    {
        Item1,
        [EnumMember(Value = "Item2:SubItem1")]
        Item2SubItem1,
        [EnumMember(Value = "Item3:SubItem1")]
        Item3SubItem1
    }
}
