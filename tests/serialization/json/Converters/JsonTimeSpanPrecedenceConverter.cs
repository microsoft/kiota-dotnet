using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Kiota.Serialization.Json.Tests.Converters;

public class JsonTimeSpanPrecedenceConverter(TimeSpan source, TimeSpan replacement) : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = TimeSpan.Parse(reader.GetString()!);
        return value == source ? replacement : value;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
