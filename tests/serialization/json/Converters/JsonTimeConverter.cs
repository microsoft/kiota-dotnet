using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Serialization.Json.Tests.Converters;

/// <summary>
/// Converts a Time object or value to/from JSON.
/// </summary>
public class JsonTimeConverter : JsonConverter<Time>
{
    /// <inheritdoc />
    public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null
            ? new Time()
            : ReadInternal(ref reader);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
        => WriteInternal(writer, value);

    private static Time ReadInternal(ref Utf8JsonReader reader)
        => new Time(DateTime.ParseExact(reader.GetString() ?? "", "HH__mm__ss", CultureInfo.InvariantCulture));

    private static void WriteInternal(Utf8JsonWriter writer, Time value)
        => writer.WriteStringValue(value.ToString());
}