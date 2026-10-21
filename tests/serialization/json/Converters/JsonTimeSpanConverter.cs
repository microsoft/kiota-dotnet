using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Kiota.Serialization.Json.Tests.Converters;

/// <summary>
/// Converts a TimeSpan object or value to/from JSON.
/// </summary>
public class JsonTimeSpanConverter : JsonConverter<TimeSpan>
{
    private const string Format = @"d\|hh\|mm\|ss";

    /// <inheritdoc />
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null
            ? TimeSpan.Zero
            : ReadInternal(ref reader);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        => WriteInternal(writer, value);

    private static TimeSpan ReadInternal(ref Utf8JsonReader reader)
        => TimeSpan.ParseExact(reader.GetString()!, Format, CultureInfo.InvariantCulture);

    private static void WriteInternal(Utf8JsonWriter writer, TimeSpan value)
        => writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
}
