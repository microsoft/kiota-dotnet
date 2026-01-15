// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml;
using Microsoft.IO;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Helpers;
using Microsoft.Kiota.Abstractions.Serialization;

#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Serialization.Json
{
    /// <summary>
    /// The <see cref="ISerializationWriter"/> implementation for json content types.
    /// </summary>
    public class JsonSerializationWriter : ISerializationWriter, IDisposable
    {
        private static readonly RecyclableMemoryStreamManager _memoryStreamManager = new RecyclableMemoryStreamManager();
        private readonly RecyclableMemoryStream _stream = _memoryStreamManager.GetStream();
        private readonly KiotaJsonSerializationContext _kiotaJsonSerializationContext;

        /// <summary>
        /// The <see cref="Utf8JsonWriter"/> instance for writing json content
        /// </summary>
        public readonly Utf8JsonWriter writer;

        /// <summary>
        /// The <see cref="JsonSerializationWriter"/> constructor
        /// </summary>
        public JsonSerializationWriter()
            : this(KiotaJsonSerializationContext.Default)
        {
        }

        /// <summary>
        /// The <see cref="JsonSerializationWriter"/> constructor
        /// </summary>
        /// <param name="kiotaJsonSerializationContext">The KiotaJsonSerializationContext to use.</param>
        public JsonSerializationWriter(KiotaJsonSerializationContext kiotaJsonSerializationContext)
        {
            _kiotaJsonSerializationContext = kiotaJsonSerializationContext;
#if netstandard2_1 || NET_CORE_APP || NET_5_OR_GREATER
            writer = new Utf8JsonWriter((IBufferWriter<byte>)_stream, new JsonWriterOptions
            {
                Encoder = kiotaJsonSerializationContext.Options.Encoder,
                Indented = kiotaJsonSerializationContext.Options.WriteIndented
            });
#else
            writer = new Utf8JsonWriter((Stream)_stream, new JsonWriterOptions
            {
                Encoder = kiotaJsonSerializationContext.Options.Encoder,
                Indented = kiotaJsonSerializationContext.Options.WriteIndented
            });
#endif
        }

        /// <summary>
        /// The action to perform before object serialization
        /// </summary>
        public Action<IParsable>? OnBeforeObjectSerialization { get; set; }

        /// <summary>
        /// The action to perform after object serialization
        /// </summary>
        public Action<IParsable>? OnAfterObjectSerialization { get; set; }

        /// <summary>
        /// The action to perform on the start of object serialization
        /// </summary>
        public Action<IParsable, ISerializationWriter>? OnStartObjectSerialization { get; set; }

        /// <summary>
        /// Get the stream of the serialized content
        /// </summary>
        /// <returns>The <see cref="Stream"/> of the serialized content</returns>
        public Stream GetSerializedContent()
        {
            writer.Flush();
            _stream.Position = 0;
            return _stream;
        }

        /// <summary>
        /// Write the string value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The string value</param>
        public void WriteStringValue(string? key, string? value)
        {
            if(value != null)
            {
                // we want to keep empty string because they are meaningful
                if(!string.IsNullOrEmpty(key))
                    writer.WritePropertyName(key!);
                JsonSerializer.Serialize(writer, value, TypeConstants.StringType, _kiotaJsonSerializationContext);
            }
        }

        /// <summary>
        /// Write the boolean value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The boolean value</param>
        public void WriteBoolValue(string? key, bool? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value.Value, TypeConstants.BooleanType, _kiotaJsonSerializationContext);
        }

        /// <summary>
        /// Write the byte value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The byte value</param>
        public void WriteByteValue(string? key, byte? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value.Value, TypeConstants.ByteType, _kiotaJsonSerializationContext);
        }

        /// <summary>
        /// Write the sbyte value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The sbyte value</param>
        public void WriteSbyteValue(string? key, sbyte? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value.Value, TypeConstants.SbyteType, _kiotaJsonSerializationContext);
        }

        /// <summary>
        /// Write the int value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The int value</param>
        public void WriteIntValue(string? key, int? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value, TypeConstants.IntType, _kiotaJsonSerializationContext);
        }

        /// <summary>
        /// Write the float value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The float value</param>
        public void WriteFloatValue(string? key, float? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value.Value, TypeConstants.FloatType, _kiotaJsonSerializationContext);
        }

        /// <summary>
        /// Write the long value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The long value</param>
        public void WriteLongValue(string? key, long? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value.Value, TypeConstants.LongType, _kiotaJsonSerializationContext);
        }

        /// <summary>
        /// Write the double value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The double value</param>
        public void WriteDoubleValue(string? key, double? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value.Value, TypeConstants.DoubleType, _kiotaJsonSerializationContext);

        }

        /// <summary>
        /// Write the decimal value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The decimal value</param>
        public void WriteDecimalValue(string? key, decimal? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value.Value, TypeConstants.DecimalType, _kiotaJsonSerializationContext);

        }

        /// <summary>
        /// Write the Guid value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The Guid value</param>
        public void WriteGuidValue(string? key, Guid? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value.Value, TypeConstants.GuidType, _kiotaJsonSerializationContext);
        }

        /// <summary>
        /// Write the DateTimeOffset value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The DateTimeOffset value</param>
        public void WriteDateTimeOffsetValue(string? key, DateTimeOffset? value)
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
                JsonSerializer.Serialize(writer, value.Value, TypeConstants.DateTimeOffsetType, _kiotaJsonSerializationContext);
        }

        /// <summary>
        /// Write the TimeSpan(An ISO8601 duration.For example, PT1M is "period time of 1 minute") value.
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The TimeSpan value</param>
        public void WriteTimeSpanValue(string? key, TimeSpan? value)
        {
            if(value.HasValue)
                WriteStringValue(key, XmlConvert.ToString(value.Value));
        }

        /// <summary>
        /// Write the Date value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The Date value</param>
        public void WriteDateValue(string? key, Date? value)
            => WriteStringValue(key, value?.ToString());

        /// <summary>
        /// Write the Time value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The Time value</param>
        public void WriteTimeValue(string? key, Time? value)
            => WriteStringValue(key, value?.ToString());

        /// <summary>
        /// Write the null value
        /// </summary>
        /// <param name="key">The key of the json node</param>
        public void WriteNullValue(string? key)
        {
            if(!string.IsNullOrEmpty(key))
                writer.WritePropertyName(key!);
            writer.WriteNullValue();
        }

        /// <summary>
        /// Write the enumeration value of type  <typeparam name="T"/>
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The enumeration value</param>
#if NET5_0_OR_GREATER
        public void WriteEnumValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(string? key, T? value) where T : struct, Enum
#else
        public void WriteEnumValue<T>(string? key, T? value) where T : struct, Enum
#endif
        {
            if(!string.IsNullOrEmpty(key) && value.HasValue)
                writer.WritePropertyName(key!);
            if(value.HasValue)
            {
                if(typeof(T).IsDefined(typeof(FlagsAttribute)))
                {
                    var values =
#if NET5_0_OR_GREATER
                        Enum.GetValues<T>();
#else
                        (T[])Enum.GetValues(typeof(T));
#endif
                    StringBuilder valueNames = new StringBuilder();
                    foreach(var x in values)
                    {
                        if(value.Value.HasFlag(x) && EnumHelpers.GetEnumStringValue(x) is string valueName)
                        {
                            if(valueNames.Length > 0)
                                valueNames.Append(",");
                            valueNames.Append(valueName);
                        }
                    }
                    WriteStringValue(null, valueNames.ToString());
                }
                else WriteStringValue(null, EnumHelpers.GetEnumStringValue(value.Value));
            }
        }

        /// <summary>
        /// Write the collection of primitives of type  <typeparam name="T"/>
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="values">The primitive collection</param>
        public void WriteCollectionOfPrimitiveValues<T>(string? key, IEnumerable<T>? values) => WriteCollectionOfPrimitiveValuesInternal(key, values);

        private void WriteCollectionOfPrimitiveValuesInternal(string? key, IEnumerable? values)
        {
            if(values != null)
            { //empty array is meaningful
                if(!string.IsNullOrEmpty(key))
                    writer.WritePropertyName(key!);
                writer.WriteStartArray();
                foreach(var collectionValue in values)
                    WriteAnyValue(null, collectionValue);
                writer.WriteEndArray();
            }
        }

        /// <summary>
        /// Write the collection of objects of type  <typeparam name="T"/>
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="values">The object collection</param>
        public void WriteCollectionOfObjectValues<T>(string? key, IEnumerable<T>? values) where T : IParsable
        {
            if(values != null)
            {
                // empty array is meaningful
                if(!string.IsNullOrEmpty(key))
                    writer.WritePropertyName(key!);
                writer.WriteStartArray();
                foreach(var item in values)
                    WriteObjectValue<T>(null, item);
                writer.WriteEndArray();
            }
        }
        /// <summary>
        /// Writes the specified collection of enum values to the stream with an optional given key.
        /// </summary>
        /// <param name="key">The key to be used for the written value. May be null.</param>
        /// <param name="values">The enum values to be written.</param>
#if NET5_0_OR_GREATER
        public void WriteCollectionOfEnumValues<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(string? key, IEnumerable<T?>? values) where T : struct, Enum
#else
        public void WriteCollectionOfEnumValues<T>(string? key, IEnumerable<T?>? values) where T : struct, Enum
#endif
        {
            if(values != null)
            { //empty array is meaningful
                if(!string.IsNullOrEmpty(key))
                    writer.WritePropertyName(key!);
                writer.WriteStartArray();
                foreach(var item in values)
                    WriteEnumValue<T>(null, item);
                writer.WriteEndArray();
            }
        }
        /// <summary>
        /// Writes the specified dictionary to the stream with an optional given key.
        /// </summary>
        /// <param name="key">The key to be used for the written value. May be null.</param>
        /// <param name="values">The dictionary of values to be written.</param>
#if NET5_0_OR_GREATER
        private void WriteDictionaryValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(string? key, T values) where T : IDictionary
#else
        private void WriteDictionaryValue<T>(string? key, T values) where T : IDictionary
#endif
        {
            if(values != null)
            {
                if(!string.IsNullOrEmpty(key))
                    writer.WritePropertyName(key!);

                writer.WriteStartObject();
                foreach(DictionaryEntry entry in values)
                {
                    if(entry.Key is not string keyStr)
                        throw new InvalidOperationException($"Error serializing dictionary value with key {key}, only string keyed dictionaries are supported.");
                    WriteAnyValue(keyStr, entry.Value);
                }
                writer.WriteEndObject();
            }
        }
        /// <summary>
        /// Writes the specified byte array as a base64 string to the stream with an optional given key.
        /// </summary>
        /// <param name="key">The key to be used for the written value. May be null.</param>
        /// <param name="value">The byte array to be written.</param>
        public void WriteByteArrayValue(string? key, byte[]? value)
        {
            //empty array is meaningful
            if(value != null)
            {
                if(string.IsNullOrEmpty(key))
                    writer.WriteBase64StringValue(value);
                else
                    writer.WriteBase64String(key!, value);
            }
        }

        /// <summary>
        /// Write the object of type <typeparam name="T"/>
        /// </summary>
        /// <param name="key">The key of the json node</param>
        /// <param name="value">The object instance to write</param>
        /// <param name="additionalValuesToMerge">The additional values to merge to the main value when serializing an intersection wrapper.</param>
        public void WriteObjectValue<T>(string? key, T? value, params IParsable?[] additionalValuesToMerge) where T : IParsable
        {
            var filteredAdditionalValuesToMerge = (IParsable[])Array.FindAll(additionalValuesToMerge, static x => x is not null);
            if(value != null || filteredAdditionalValuesToMerge.Length > 0)
            {
                // until interface exposes WriteUntypedValue()
                var serializingUntypedNode = value is UntypedNode;
                if(!serializingUntypedNode && !string.IsNullOrEmpty(key))
                    writer.WritePropertyName(key!);
                if(value != null)
                    OnBeforeObjectSerialization?.Invoke(value);

                if(serializingUntypedNode)
                {
                    var untypedNode = value as UntypedNode;
                    OnStartObjectSerialization?.Invoke(untypedNode!, this);
                    WriteUntypedValue(key, untypedNode);
                    OnAfterObjectSerialization?.Invoke(untypedNode!);
                }
                else
                {
                    var serializingScalarValue = value is IComposedTypeWrapper;
                    if(!serializingScalarValue)
                        writer.WriteStartObject();
                    if(value != null)
                    {
                        OnStartObjectSerialization?.Invoke(value, this);
                        value.Serialize(this);
                    }
                    foreach(var additionalValueToMerge in filteredAdditionalValuesToMerge)
                    {
                        OnBeforeObjectSerialization?.Invoke(additionalValueToMerge!);
                        OnStartObjectSerialization?.Invoke(additionalValueToMerge!, this);
                        additionalValueToMerge!.Serialize(this);
                        OnAfterObjectSerialization?.Invoke(additionalValueToMerge);
                    }
                    if(!serializingScalarValue)
                        writer.WriteEndObject();
                }
                if(value != null) OnAfterObjectSerialization?.Invoke(value);
            }
        }

        /// <summary>
        /// Write the additional data property bag
        /// </summary>
        /// <param name="value">The additional data dictionary</param>
        public void WriteAdditionalData(IDictionary<string, object> value)
        {
            if(value == null)
                return;

            foreach(var dataValue in value)
                WriteAnyValue(dataValue.Key, dataValue.Value);
        }

#if NET5_0_OR_GREATER
        private void WriteNonParsableObjectValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(string? key, T value)
#else
        private void WriteNonParsableObjectValue<T>(string? key, T value)
#endif
        {
            if(!string.IsNullOrEmpty(key))
                writer.WritePropertyName(key!);
            writer.WriteStartObject();
            if(value == null)
                writer.WriteNullValue();
            else
                foreach(var oProp in value.GetType().GetProperties())
                    WriteAnyValue(oProp.Name, oProp.GetValue(value));
            writer.WriteEndObject();
        }

        private void WriteAnyValue<T>(string? key, T value)
        {
            switch(value)
            {
                case string s:
                    WriteStringValue(key, s);
                    break;
                case bool b:
                    WriteBoolValue(key, b);
                    break;
                case byte b:
                    WriteByteValue(key, b);
                    break;
                case sbyte b:
                    WriteSbyteValue(key, b);
                    break;
                case int i:
                    WriteIntValue(key, i);
                    break;
                case float f:
                    WriteFloatValue(key, f);
                    break;
                case long l:
                    WriteLongValue(key, l);
                    break;
                case double d:
                    WriteDoubleValue(key, d);
                    break;
                case decimal dec:
                    WriteDecimalValue(key, dec);
                    break;
                case Guid g:
                    WriteGuidValue(key, g);
                    break;
                case DateTimeOffset dto:
                    WriteDateTimeOffsetValue(key, dto);
                    break;
                case TimeSpan timeSpan:
                    WriteTimeSpanValue(key, timeSpan);
                    break;
                case UntypedNode node:
                    WriteUntypedValue(key, node);
                    break;
                case IParsable parseable:
                    WriteObjectValue(key, parseable);
                    break;
                case Date date:
                    WriteDateValue(key, date);
                    break;
                case DateTime dateTime:
                    WriteDateTimeOffsetValue(key, new DateTimeOffset(dateTime));
                    break;
                case Time time:
                    WriteTimeValue(key, time);
                    break;
                case JsonElement jsonElement:
                    if(!string.IsNullOrEmpty(key))
                        writer.WritePropertyName(key!);
                    jsonElement.WriteTo(writer);
                    break;
                case IDictionary dictionary:
                    WriteDictionaryValue(key, dictionary);
                    break;
                case IEnumerable coll:
                    WriteCollectionOfPrimitiveValuesInternal(key, coll);
                    break;
                case object o:
                    WriteNonParsableObjectValue(key, o);
                    break;
                case null:
                    WriteNullValue(key);
                    break;
                default:
                    throw new InvalidOperationException($"Error serializing additional data value with key {key}, unknown type {value?.GetType()}");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            writer.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Writes a untyped value for the specified key.
        /// </summary>
        /// <param name="key">The key to be used for the written value. May be null.</param>
        /// <param name="value">The untyped node.</param>
        private void WriteUntypedValue(string? key, UntypedNode? value)
        {
            switch(value)
            {
                case UntypedString untypedString:
                    WriteStringValue(key, untypedString.GetValue());
                    break;
                case UntypedBoolean untypedBoolean:
                    WriteBoolValue(key, untypedBoolean.GetValue());
                    break;
                case UntypedInteger untypedInteger:
                    WriteIntValue(key, untypedInteger.GetValue());
                    break;
                case UntypedLong untypedLong:
                    WriteLongValue(key, untypedLong.GetValue());
                    break;
                case UntypedDecimal untypedDecimal:
                    WriteDecimalValue(key, untypedDecimal.GetValue());
                    break;
                case UntypedFloat untypedFloat:
                    WriteFloatValue(key, untypedFloat.GetValue());
                    break;
                case UntypedDouble untypedDouble:
                    WriteDoubleValue(key, untypedDouble.GetValue());
                    break;
                case UntypedObject untypedObject:
                    WriteUntypedObject(key, untypedObject);
                    break;
                case UntypedArray array:
                    WriteUntypedArray(key, array);
                    break;
                case UntypedNull:
                    WriteNullValue(key);
                    break;
            }
        }

        /// <summary>
        /// Write a untyped object for the specified key.
        /// </summary>
        /// <param name="key">The key to be used for the written value. May be null.</param>
        /// <param name="value">The untyped object.</param>
        private void WriteUntypedObject(string? key, UntypedObject? value)
        {
            if(value != null)
            {
                if(!string.IsNullOrEmpty(key)) writer.WritePropertyName(key!);
                writer.WriteStartObject();
                foreach(var item in value.GetValue())
                    WriteUntypedValue(item.Key, item.Value);
                writer.WriteEndObject();
            }
        }

        /// <summary>
        /// Writes the specified collection of untyped values.
        /// </summary>
        /// <param name="key">The key to be used for the written value. May be null.</param>
        /// <param name="array">The collection of untyped values.</param>
        private void WriteUntypedArray(string? key, UntypedArray? array)
        {
            if(array != null)
            {
                if(!string.IsNullOrEmpty(key)) writer.WritePropertyName(key!);
                writer.WriteStartArray();
                foreach(var item in array.GetValue())
                    WriteUntypedValue(null, item);
                writer.WriteEndArray();
            }
        }
    }
}
