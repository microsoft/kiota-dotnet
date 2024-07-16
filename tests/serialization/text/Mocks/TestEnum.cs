using System.Runtime.Serialization;

namespace Microsoft.Kiota.Serialization.Text.Tests.Mocks
{
    public enum TestEnum
    {
        [EnumMember(Value = "Value_1")]
        FirstItem,
        [EnumMember(Value = "Value_2")]
        SecondItem,
    }
}
