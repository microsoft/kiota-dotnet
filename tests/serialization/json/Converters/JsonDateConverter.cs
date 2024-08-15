using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Serialization.Json.Tests.Converters;

/// <summary>
/// Converts a Date object or value to/from JSON.
/// </summary>
public class JsonDateConverter : JsonConverter<Date>
{
    /// <inheritdoc />
    public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null
            ? new Date()
            : ReadInternal(ref reader);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Date value, JsonSerializerOptions options)
        => WriteInternal(writer, value);

    private static Date ReadInternal(ref Utf8JsonReader reader)
        => new Date(DateTime.ParseExact(reader.GetString()!, "dd---MM---yyyy", CultureInfo.InvariantCulture));

    private static void WriteInternal(Utf8JsonWriter writer, Date value)
        => writer.WriteStringValue(value.ToString());
}