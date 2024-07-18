using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Kiota.Serialization.Json.Tests.Converters;

/// <summary>
/// Converts a DateTimeOffset object or value to/from JSON.
/// </summary>
public class JsonDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    /// <inheritdoc />
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null
            ? new DateTimeOffset()
            : ReadInternal(ref reader);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => WriteInternal(writer, value);

    private static DateTimeOffset ReadInternal(ref Utf8JsonReader reader)
        => DateTimeOffset.ParseExact(reader.GetString()!, "dd__MM__yyyyTHH_mm_ssZ", CultureInfo.InvariantCulture);

    private static void WriteInternal(Utf8JsonWriter writer, DateTimeOffset value)
        => writer.WriteStringValue(value.ToString("dd__MM__yyyyTHH_mm_ssZ"));
}
