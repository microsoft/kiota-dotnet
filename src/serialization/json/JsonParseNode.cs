// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Xml;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Helpers;
using Microsoft.Kiota.Abstractions.Serialization;


#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Serialization.Json
{
    /// <summary>
    /// The <see cref="IParseNode"/> implementation for the json content type
    /// </summary>
    public class JsonParseNode : IParseNode
    {
        private readonly JsonElement _jsonNode;
        private readonly KiotaJsonSerializationContext _jsonSerializerContext;

        /// <summary>
        /// The <see cref="JsonParseNode"/> constructor.
        /// </summary>
        /// <param name="node">The JsonElement to initialize the node with</param>
        public JsonParseNode(JsonElement node)
            : this(node, KiotaJsonSerializationContext.Default)
        {
        }

        /// <summary>
        /// The <see cref="JsonParseNode"/> constructor.
        /// </summary>
        /// <param name="node">The JsonElement to initialize the node with</param>
        /// <param name="jsonSerializerContext">The JsonSerializerContext to utilize.</param>
        public JsonParseNode(JsonElement node, KiotaJsonSerializationContext jsonSerializerContext)
        {
            _jsonNode = node;
            _jsonSerializerContext = jsonSerializerContext;
        }

        /// <summary>
        /// Get the string value from the json node
        /// </summary>
        /// <returns>A string value</returns>
        public string? GetStringValue() => GetStringValue(_jsonNode);

        /// <summary>
        /// Get the boolean value from the json node
        /// </summary>
        /// <returns>A boolean value</returns>
        public bool? GetBoolValue() => GetBoolValue(_jsonNode);

        /// <summary>
        /// Get the byte value from the json node
        /// </summary>
        /// <returns>A byte value</returns>
        public byte? GetByteValue() => GetByteValue(_jsonNode);

        /// <summary>
        /// Get the sbyte value from the json node
        /// </summary>
        /// <returns>A sbyte value</returns>
        public sbyte? GetSbyteValue() => GetSbyteValue(_jsonNode);

        /// <summary>
        /// Get the int value from the json node
        /// </summary>
        /// <returns>A int value</returns>
        public int? GetIntValue() => GetIntValue(_jsonNode);


        /// <summary>
        /// Get the float value from the json node
        /// </summary>
        /// <returns>A float value</returns>
        public float? GetFloatValue() => GetFloatValue(_jsonNode);

        /// <summary>
        /// Get the Long value from the json node
        /// </summary>
        /// <returns>A Long value</returns>
        public long? GetLongValue() => GetLongValue(_jsonNode);

        /// <summary>
        /// Get the double value from the json node
        /// </summary>
        /// <returns>A double value</returns>
        public double? GetDoubleValue() => GetDoubleValue(_jsonNode);

        /// <summary>
        /// Get the decimal value from the json node
        /// </summary>
        /// <returns>A decimal value</returns>
        public decimal? GetDecimalValue() => GetDecimalValue(_jsonNode);

        private static bool ShouldBeDeserializableNumber(JsonValueKind valueKind, JsonNumberHandling numberHandling) => valueKind switch
        {
            JsonValueKind.Number => true,
            JsonValueKind.String when numberHandling.HasFlag(JsonNumberHandling.AllowReadingFromString) => true,
            _ => false
        };

        /// <summary>
        /// Get the guid value from the json node
        /// </summary>
        /// <returns>A guid value</returns>
        public Guid? GetGuidValue()
        {
            return GetGuidValue(_jsonNode);
        }

        /// <summary>
        /// Get the <see cref="DateTimeOffset"/> value from the json node
        /// </summary>
        /// <returns>A <see cref="DateTimeOffset"/> value</returns>
        public DateTimeOffset? GetDateTimeOffsetValue()
        {
            return GetDateTimeOffsetValue(_jsonNode);
        }

        /// <summary>
        /// Get the <see cref="TimeSpan"/> value from the json node
        /// </summary>
        /// <returns>A <see cref="TimeSpan"/> value</returns>
        public TimeSpan? GetTimeSpanValue()
        {
            return GetTimeSpanValue(_jsonNode);
        }

        /// <summary>
        /// Get the <see cref="Date"/> value from the json node
        /// </summary>
        /// <returns>A <see cref="Date"/> value</returns>
        public Date? GetDateValue()
        {
            return GetDateValue(_jsonNode);
        }

        /// <summary>
        /// Get the <see cref="Time"/> value from the json node
        /// </summary>
        /// <returns>A <see cref="Time"/> value</returns>
        public Time? GetTimeValue()
        {
            return GetTimeValue(_jsonNode);
        }

        /// <summary>
        /// Get the enumeration value of type <typeparam name="T"/>from the json node
        /// </summary>
        /// <returns>An enumeration value or null</returns>
#if NET5_0_OR_GREATER
        public T? GetEnumValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>() where T : struct, Enum
#else
        public T? GetEnumValue<T>() where T : struct, Enum
#endif
        {
            var rawValue = _jsonNode.GetString();
            return EnumHelpers.GetEnumValue<T>(rawValue!);
        }

        /// <summary>
        /// Get the collection of type <typeparam name="T"/>from the json node
        /// </summary>
        /// <param name="factory">The factory to use to create the model object.</param>
        /// <returns>A collection of objects</returns>
        public IEnumerable<T> GetCollectionOfObjectValues<T>(ParsableFactory<T> factory) where T : IParsable
        {
            if(_jsonNode.ValueKind == JsonValueKind.Array)
            {
                var enumerator = _jsonNode.EnumerateArray();
                while(enumerator.MoveNext())
                {
                    var currentParseNode = new JsonParseNode(enumerator.Current, _jsonSerializerContext)
                    {
                        OnAfterAssignFieldValues = OnAfterAssignFieldValues,
                        OnBeforeAssignFieldValues = OnBeforeAssignFieldValues
                    };
                    yield return currentParseNode.GetObjectValue<T>(factory);
                }
            }
        }
        /// <summary>
        /// Gets the collection of enum values of the node.
        /// </summary>
        /// <returns>The collection of enum values.</returns>
#if NET5_0_OR_GREATER
        public IEnumerable<T?> GetCollectionOfEnumValues<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>() where T : struct, Enum
#else
        public IEnumerable<T?> GetCollectionOfEnumValues<T>() where T : struct, Enum
#endif
        {
            if(_jsonNode.ValueKind == JsonValueKind.Array)
            {
                var enumerator = _jsonNode.EnumerateArray();
                while(enumerator.MoveNext())
                {
                    yield return GetEnumValue<T>(enumerator.Current);
                }
            }
        }
        /// <summary>
        /// Gets the byte array value of the node.
        /// </summary>
        /// <returns>The byte array value of the node.</returns>
        public byte[]? GetByteArrayValue()
        {
            if(_jsonNode.ValueKind is JsonValueKind.String && _jsonNode.TryGetBytesFromBase64(out var result))
                return result;
            return null;
        }
        /// <summary>
        /// Gets the untyped value of the node
        /// </summary>
        /// <returns>The untyped value of the node.</returns>
        private UntypedNode GetUntypedValue() => GetUntypedValue(_jsonNode);


        /// <summary>
        /// Get the collection of primitives of type <typeparam name="T"/>from the json node
        /// </summary>
        /// <returns>A collection of objects</returns>
#if NET5_0_OR_GREATER
        public IEnumerable<T> GetCollectionOfPrimitiveValues<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>()
#else
        public IEnumerable<T> GetCollectionOfPrimitiveValues<T>()
#endif
        {
            if(_jsonNode.ValueKind == JsonValueKind.Array)
            {
                var genericType = typeof(T);
                foreach(var collectionValue in _jsonNode.EnumerateArray())
                {
                    if(genericType == TypeConstants.BooleanType)
                        yield return (T)(object)GetBoolValue(collectionValue)!;
                    else if(genericType == TypeConstants.ByteType)
                        yield return (T)(object)GetByteValue(collectionValue)!;
                    else if(genericType == TypeConstants.SbyteType)
                        yield return (T)(object)GetSbyteValue(collectionValue)!;
                    else if(genericType == TypeConstants.StringType)
                        yield return (T)(object)GetStringValue(collectionValue)!;
                    else if(genericType == TypeConstants.IntType)
                        yield return (T)(object)GetIntValue(collectionValue)!;
                    else if(genericType == TypeConstants.FloatType)
                        yield return (T)(object)GetFloatValue(collectionValue)!;
                    else if(genericType == TypeConstants.LongType)
                        yield return (T)(object)GetLongValue(collectionValue)!;
                    else if(genericType == TypeConstants.DoubleType)
                        yield return (T)(object)GetDoubleValue(collectionValue)!;
                    else if(genericType == TypeConstants.GuidType)
                        yield return (T)(object)GetGuidValue(collectionValue)!;
                    else if(genericType == TypeConstants.DateTimeOffsetType)
                        yield return (T)(object)GetDateTimeOffsetValue(collectionValue)!;
                    else if(genericType == TypeConstants.TimeSpanType)
                        yield return (T)(object)GetTimeSpanValue(collectionValue)!;
                    else if(genericType == TypeConstants.DateType)
                        yield return (T)(object)GetDateValue(collectionValue)!;
                    else if(genericType == TypeConstants.TimeType)
                        yield return (T)(object)GetTimeValue(collectionValue)!;
                    else if(GetStringValue(collectionValue) is { Length: > 0 } rawValue)
                    {
                        yield return (T)EnumHelpers.GetEnumValue(genericType, rawValue)!;
                    }
                    else
                        throw new InvalidOperationException($"unknown type for deserialization {genericType.FullName}");
                }
            }
        }

        private string? GetStringValue(JsonElement jsonElement) => jsonElement.ValueKind == JsonValueKind.String
            ? jsonElement.Deserialize(_jsonSerializerContext.String)
            : null;

        private bool? GetBoolValue(JsonElement jsonElement) =>
            jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False
                ? jsonElement.Deserialize(_jsonSerializerContext.Boolean)
                : null;

        private byte? GetByteValue(JsonElement jsonElement) => jsonElement.ValueKind == JsonValueKind.Number
            ? jsonElement.Deserialize(_jsonSerializerContext.Byte)
            : null;

        private sbyte? GetSbyteValue(JsonElement jsonElement) => jsonElement.ValueKind == JsonValueKind.Number
            ? jsonElement.Deserialize(_jsonSerializerContext.SByte)
            : null;

        private int? GetIntValue(JsonElement jsonElement) => ShouldBeDeserializableNumber(jsonElement.ValueKind, _jsonSerializerContext.Options.NumberHandling)
            ? jsonElement.Deserialize(_jsonSerializerContext.Int32)
            : null;

        private float? GetFloatValue(JsonElement jsonElement) => ShouldBeDeserializableNumber(jsonElement.ValueKind, _jsonSerializerContext.Options.NumberHandling)
            ? jsonElement.Deserialize(_jsonSerializerContext.Single)
            : null;

        private long? GetLongValue(JsonElement jsonElement) => ShouldBeDeserializableNumber(jsonElement.ValueKind, _jsonSerializerContext.Options.NumberHandling)
            ? jsonElement.Deserialize(_jsonSerializerContext.Int64)
            : null;

        private double? GetDoubleValue(JsonElement jsonElement) => ShouldBeDeserializableNumber(jsonElement.ValueKind, _jsonSerializerContext.Options.NumberHandling)
            ? jsonElement.Deserialize(_jsonSerializerContext.Double)
            : null;

        private decimal? GetDecimalValue(JsonElement jsonElement) => ShouldBeDeserializableNumber(jsonElement.ValueKind, _jsonSerializerContext.Options.NumberHandling)
            ? jsonElement.Deserialize(_jsonSerializerContext.Decimal)
            : null;

        private Guid? GetGuidValue(JsonElement jsonElement)
        {
            if(jsonElement.ValueKind != JsonValueKind.String)
                return null;

            if(jsonElement.TryGetGuid(out var guid))
                return guid;

            if(string.IsNullOrEmpty(jsonElement.GetString()))
                return null;

            return jsonElement.Deserialize(_jsonSerializerContext.Guid);
        }

        private DateTimeOffset? GetDateTimeOffsetValue(JsonElement jsonElement)
        {
            if(jsonElement.ValueKind != JsonValueKind.String)
                return null;

            if(jsonElement.TryGetDateTimeOffset(out var dateTimeOffset))
                return dateTimeOffset;

            if(TryGetUsingTypeInfo(jsonElement, _jsonSerializerContext.DateTimeOffset, out var convertedDateTimeOffset))
                return convertedDateTimeOffset;

            if(DateTimeOffset.TryParse(jsonElement.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
                return dto;

            return null;
        }

        private static TimeSpan? GetTimeSpanValue(JsonElement jsonElement)
        {
            var jsonString = jsonElement.GetString();
            if(string.IsNullOrEmpty(jsonString))
                return null;

            // Parse an ISO8601 duration.http://en.wikipedia.org/wiki/ISO_8601#Durations to a TimeSpan
            return XmlConvert.ToTimeSpan(jsonString);
        }

        private Date? GetDateValue(JsonElement jsonElement)
        {
            if(jsonElement.ValueKind != JsonValueKind.String)
                return null;

            if(TryGetUsingTypeInfo(jsonElement, _jsonSerializerContext.Date, out var date))
                return date;
            if(DateTime.TryParse(jsonElement.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
                return new Date(dt);

            return null;
        }

        private Time? GetTimeValue(JsonElement jsonElement)
        {
            if(jsonElement.ValueKind != JsonValueKind.String)
                return null;

            if(TryGetUsingTypeInfo(jsonElement, _jsonSerializerContext.Time, out var time))
                return time;
            if(DateTime.TryParse(jsonElement.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
                return new Time(result);

            return null;
        }

#if NET5_0_OR_GREATER
        private T? GetEnumValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>(JsonElement jsonElement) where T : struct, Enum
#else
        private T? GetEnumValue<T>(JsonElement jsonElement) where T : struct, Enum
#endif
        {
            var rawValue = GetStringValue(jsonElement);
            return EnumHelpers.GetEnumValue<T>(rawValue!);
        }

        /// <summary>
        /// Gets the collection of untyped values of the node.
        /// </summary>
        /// <returns>The collection of untyped values.</returns>
        private IEnumerable<UntypedNode> GetCollectionOfUntypedValues(JsonElement jsonNode)
        {
            if(jsonNode.ValueKind == JsonValueKind.Array)
            {
                foreach(var collectionValue in jsonNode.EnumerateArray())
                {
                    yield return GetUntypedValue(collectionValue);
                }
            }
        }

        /// <summary>
        /// Gets the collection of properties in the untyped object.
        /// </summary>
        /// <returns>The collection of properties in the untyped object.</returns>
        private IDictionary<string, UntypedNode> GetPropertiesOfUntypedObject(JsonElement jsonNode)
        {
            var properties = new Dictionary<string, UntypedNode>();
            if(jsonNode.ValueKind == JsonValueKind.Object)
            {
                foreach(var objectValue in jsonNode.EnumerateObject())
                {
                    JsonElement property = objectValue.Value;
                    if(objectValue.Value.ValueKind == JsonValueKind.Object)
                    {
                        var objectVal = GetPropertiesOfUntypedObject(objectValue.Value);
                        properties[objectValue.Name] = new UntypedObject(objectVal);
                    }
                    else
                    {
                        properties[objectValue.Name] = GetUntypedValue(property);
                    }
                }
            }
            return properties;
        }

        private UntypedNode GetUntypedValue(JsonElement jsonNode) => jsonNode.ValueKind switch
        {
            JsonValueKind.Number when jsonNode.TryGetInt32(out var intValue) => new UntypedInteger(intValue),
            JsonValueKind.Number when jsonNode.TryGetInt64(out var longValue) => new UntypedLong(longValue),
            JsonValueKind.Number when jsonNode.TryGetDecimal(out var decimalValue) => new UntypedDecimal(decimalValue),
            JsonValueKind.Number when jsonNode.TryGetSingle(out var floatValue) => new UntypedFloat(floatValue),
            JsonValueKind.Number when jsonNode.TryGetDouble(out var doubleValue) => new UntypedDouble(doubleValue),
            JsonValueKind.String => new UntypedString(jsonNode.GetString()),
            JsonValueKind.True or JsonValueKind.False => new UntypedBoolean(jsonNode.GetBoolean()),
            JsonValueKind.Array => new UntypedArray(GetCollectionOfUntypedValues(jsonNode)),
            JsonValueKind.Object => new UntypedObject(GetPropertiesOfUntypedObject(jsonNode)),
            JsonValueKind.Null or JsonValueKind.Undefined => new UntypedNull(),
            _ => throw new InvalidOperationException($"unexpected additional value type during deserialization json kind : {jsonNode.ValueKind}")
        };

        /// <summary>
        /// The action to perform before assigning field values.
        /// </summary>
        public Action<IParsable>? OnBeforeAssignFieldValues { get; set; }

        /// <summary>
        /// The action to perform after assigning field values.
        /// </summary>
        public Action<IParsable>? OnAfterAssignFieldValues { get; set; }

        /// <summary>
        /// Get the object of type <typeparam name="T"/>from the json node
        /// </summary>
        /// <param name="factory">The factory to use to create the model object.</param>
        /// <returns>A object of the specified type</returns>
        public T GetObjectValue<T>(ParsableFactory<T> factory) where T : IParsable
        {
            // until interface exposes GetUntypedValue()
            var genericType = typeof(T);
            if(genericType == typeof(UntypedNode))
            {
                return (T)(object)GetUntypedValue();
            }
            var item = factory(this);
            OnBeforeAssignFieldValues?.Invoke(item);
            AssignFieldValues(item);
            OnAfterAssignFieldValues?.Invoke(item);
            return item;
        }
        private void AssignFieldValues<T>(T item) where T : IParsable
        {
            if(_jsonNode.ValueKind != JsonValueKind.Object) return;
            IDictionary<string, object>? itemAdditionalData = null;
            if(item is IAdditionalDataHolder holder)
            {
                holder.AdditionalData ??= new Dictionary<string, object>();
                itemAdditionalData = holder.AdditionalData;
            }
            var fieldDeserializers = item.GetFieldDeserializers();

            foreach(var fieldValue in _jsonNode.EnumerateObject())
            {
                if(fieldDeserializers.TryGetValue(fieldValue.Name, out var fieldDeserializer))
                {
                    if(fieldValue.Value.ValueKind == JsonValueKind.Null)
                        continue;// If the property is already null just continue. As calling functions like GetDouble,GetBoolValue do not process JsonValueKind.Null.
                    Debug.WriteLine($"found property {fieldValue.Name} to deserialize");
                    fieldDeserializer.Invoke(new JsonParseNode(fieldValue.Value, _jsonSerializerContext)
                    {
                        OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                        OnAfterAssignFieldValues = OnAfterAssignFieldValues
                    });
                }
                else if(itemAdditionalData != null)
                {
                    Debug.WriteLine($"found additional property {fieldValue.Name} to deserialize");
                    IDictionaryExtensions.TryAdd(itemAdditionalData, fieldValue.Name, TryGetAnything(fieldValue.Value)!);
                }
                else
                {
                    Debug.WriteLine($"found additional property {fieldValue.Name} to deserialize but the model doesn't support additional data");
                }
            }
        }
        private object? TryGetAnything(JsonElement element)
        {
            switch(element.ValueKind)
            {
                case JsonValueKind.Number:
                    if(element.TryGetDecimal(out var dec)) return dec;
                    else if(element.TryGetDouble(out var db)) return db;
                    else if(element.TryGetInt16(out var s)) return s;
                    else if(element.TryGetInt32(out var i)) return i;
                    else if(element.TryGetInt64(out var l)) return l;
                    else if(element.TryGetSingle(out var f)) return f;
                    else if(element.TryGetUInt16(out var us)) return us;
                    else if(element.TryGetUInt32(out var ui)) return ui;
                    else if(element.TryGetUInt64(out var ul)) return ul;
                    else throw new InvalidOperationException("unexpected additional value type during number deserialization");
                case JsonValueKind.String:
                    if(element.TryGetDateTime(out var dt)) return dt;
                    else if(element.TryGetDateTimeOffset(out var dto)) return dto;
                    else if(element.TryGetGuid(out var g)) return g;
                    else return element.GetString();
                case JsonValueKind.Array:
                case JsonValueKind.Object:
                    return GetUntypedValue(element);
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                default:
                    throw new InvalidOperationException($"unexpected additional value type during deserialization json kind : {element.ValueKind}");
            }
        }

        /// <summary>
        /// Get the child node of the specified identifier
        /// </summary>
        /// <param name="identifier">The identifier of the child node</param>
        /// <returns>An instance of <see cref="IParseNode"/></returns>
        public IParseNode? GetChildNode(string identifier)
        {
            if(_jsonNode.ValueKind == JsonValueKind.Object && _jsonNode.TryGetProperty(identifier ?? throw new ArgumentNullException(nameof(identifier)), out var jsonElement))
            {
                return new JsonParseNode(jsonElement, _jsonSerializerContext)
                {
                    OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                    OnAfterAssignFieldValues = OnAfterAssignFieldValues
                };
            }

            return default;
        }

        private static bool TryGetUsingTypeInfo<T>(JsonElement currentElement, JsonTypeInfo<T>? typeInfo, out T? deserializedValue)
        {
            try
            {
                deserializedValue = currentElement.Deserialize(typeInfo!);
                return true;
            }
            catch(Exception)
            {
                deserializedValue = default;
                return false;
            }
        }
    }
}
