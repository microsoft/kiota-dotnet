using System.Runtime.Serialization;

namespace Microsoft.Kiota.Abstractions.Tests.Mocks;

public enum TestEnum
{
    [EnumMember(Value = "1")]
    First,
    [EnumMember(Value = "2")]
    Second,
}