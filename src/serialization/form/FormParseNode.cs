using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Helpers;
using Microsoft.Kiota.Abstractions.Serialization;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Serialization.Form;
/// <summary>Represents a parse node that can be used to parse a form url encoded string.</summary>
public class FormParseNode : IParseNode
{
    private readonly string RawValue;
    private string DecodedValue => Uri.UnescapeDataString(RawValue);
    private readonly Dictionary<string, string> Fields;
    private static readonly char[] pairDelimiter = ['='];
    private static readonly char[] entriesDelimiter = ['&'];
    /// <summary>Initializes a new instance of the <see cref="FormParseNode"/> class.</summary>
    /// <param name="rawValue">The raw value to parse.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="rawValue"/> is null.</exception>
    public FormParseNode(string rawValue)
    {
        RawValue = rawValue ?? throw new ArgumentNullException(nameof(rawValue));
        Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string[] pairs = rawValue.Split(entriesDelimiter, StringSplitOptions.RemoveEmptyEntries);
        foreach(string pair in pairs)
        {
            string[] keyValue = pair.Split(pairDelimiter, StringSplitOptions.RemoveEmptyEntries);
            if(keyValue.Length == 2)
            {
                string key = SanitizeKey(keyValue[0]);
                string value = keyValue[1].Trim();

                if(Fields.ContainsKey(key))
                {
                    Fields[key] += $",{value}";
                }
                else
                {
                    Fields.Add(key, value);
                }
            }
        }
    }

    private static string SanitizeKey(string key)
    {
        if(string.IsNullOrEmpty(key)) return key;
        return Uri.UnescapeDataString(key.Trim());
    }
    /// <inheritdoc/>
    public Action<IParsable>? OnBeforeAssignFieldValues { get; set; }
    /// <inheritdoc/>
    public Action<IParsable>? OnAfterAssignFieldValues { get; set; }
    /// <inheritdoc/>
    public bool? GetBoolValue() => bool.TryParse(DecodedValue, out var result) && result;
    /// <inheritdoc/>
    public byte[]? GetByteArrayValue()
    {
        var rawValue = DecodedValue;
        if(string.IsNullOrEmpty(rawValue)) return null;
        return Convert.FromBase64String(rawValue);
    }
    /// <inheritdoc/>
    public byte? GetByteValue() => byte.TryParse(DecodedValue, out var result) ? result : null;
    /// <inheritdoc/>
    public IParseNode? GetChildNode(string identifier) => Fields.TryGetValue(SanitizeKey(identifier), out var value) ?
        new FormParseNode(value)
        {
            OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
            OnAfterAssignFieldValues = OnAfterAssignFieldValues
        } : null;
    /// <inheritdoc/>
    public IEnumerable<T> GetCollectionOfObjectValues<T>(ParsableFactory<T> factory) where T : IParsable => throw new InvalidOperationException("collections are not supported with uri form encoding");

    private static readonly Type booleanType = typeof(bool?);
    private static readonly Type byteType = typeof(byte?);
    private static readonly Type sbyteType = typeof(sbyte?);
    private static readonly Type stringType = typeof(string);
    private static readonly Type intType = typeof(int?);
    private static readonly Type decimalType = typeof(decimal?);
    private static readonly Type floatType = typeof(float?);
    private static readonly Type doubleType = typeof(double?);
    private static readonly Type guidType = typeof(Guid?);
    private static readonly Type dateTimeOffsetType = typeof(DateTimeOffset?);
    private static readonly Type timeSpanType = typeof(TimeSpan?);
    private static readonly Type dateType = typeof(Date?);
    private static readonly Type timeType = typeof(Time?);
    private static readonly char[] ComaSeparator = [','];

    /// <summary>
    /// Get the collection of primitives of type <typeparam name="T"/>from the form node
    /// </summary>
    /// <returns>A collection of objects</returns>
#if NET5_0_OR_GREATER
    public IEnumerable<T> GetCollectionOfPrimitiveValues<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>()
#else
    public IEnumerable<T> GetCollectionOfPrimitiveValues<T>()
#endif
    {
        var genericType = typeof(T);
        var primitiveValueCollection = DecodedValue.Split(ComaSeparator, StringSplitOptions.RemoveEmptyEntries);
        foreach(var collectionValue in primitiveValueCollection)
        {
            var currentParseNode = new FormParseNode(collectionValue)
            {
                OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                OnAfterAssignFieldValues = OnAfterAssignFieldValues
            };
            if(genericType == booleanType)
                yield return (T)(object)currentParseNode.GetBoolValue()!;
            else if(genericType == byteType)
                yield return (T)(object)currentParseNode.GetByteValue()!;
            else if(genericType == sbyteType)
                yield return (T)(object)currentParseNode.GetSbyteValue()!;
            else if(genericType == stringType)
                yield return (T)(object)currentParseNode.GetStringValue()!;
            else if(genericType == intType)
                yield return (T)(object)currentParseNode.GetIntValue()!;
            else if(genericType == floatType)
                yield return (T)(object)currentParseNode.GetFloatValue()!;
            else if(genericType == doubleType)
                yield return (T)(object)currentParseNode.GetDoubleValue()!;
            else if(genericType == decimalType)
                yield return (T)(object)currentParseNode.GetDecimalValue()!;
            else if(genericType == guidType)
                yield return (T)(object)currentParseNode.GetGuidValue()!;
            else if(genericType == dateTimeOffsetType)
                yield return (T)(object)currentParseNode.GetDateTimeOffsetValue()!;
            else if(genericType == timeSpanType)
                yield return (T)(object)currentParseNode.GetTimeSpanValue()!;
            else if(genericType == dateType)
                yield return (T)(object)currentParseNode.GetDateValue()!;
            else if(genericType == timeType)
                yield return (T)(object)currentParseNode.GetTimeValue()!;
            else
                throw new InvalidOperationException($"unknown type for deserialization {genericType.FullName}");
        }
    }
    /// <inheritdoc/>
    public DateTimeOffset? GetDateTimeOffsetValue() => DateTimeOffset.TryParse(DecodedValue, out var result) ? result : null;
    /// <inheritdoc/>
    public Date? GetDateValue() => DateTime.TryParse(DecodedValue, out var result) ? new Date(result) : null;
    /// <inheritdoc/>
    public decimal? GetDecimalValue() => decimal.TryParse(DecodedValue, out var result) ? result : null;
    /// <inheritdoc/>
    public double? GetDoubleValue() => double.TryParse(DecodedValue, out var result) ? result : null;
    /// <inheritdoc/>
    public float? GetFloatValue() => float.TryParse(DecodedValue, out var result) ? result : null;
    /// <inheritdoc/>
    public Guid? GetGuidValue() => Guid.TryParse(DecodedValue, out var result) ? result : null;
    /// <inheritdoc/>
    public int? GetIntValue() => int.TryParse(DecodedValue, out var result) ? result : null;
    /// <inheritdoc/>
    public long? GetLongValue() => long.TryParse(DecodedValue, out var result) ? result : null;
    /// <inheritdoc/>
    public T GetObjectValue<T>(ParsableFactory<T> factory) where T : IParsable
    {
        var item = factory(this);
        OnBeforeAssignFieldValues?.Invoke(item);
        AssignFieldValues(item);
        OnAfterAssignFieldValues?.Invoke(item);
        return item;
    }
    private void AssignFieldValues<T>(T item) where T : IParsable
    {
        if(Fields.Count == 0) return;
        IDictionary<string, object>? itemAdditionalData = null;
        if(item is IAdditionalDataHolder holder)
        {
            holder.AdditionalData ??= new Dictionary<string, object>();
            itemAdditionalData = holder.AdditionalData;
        }
        var fieldDeserializers = item.GetFieldDeserializers();

        foreach(var fieldValue in Fields)
        {
            if(fieldDeserializers.TryGetValue(fieldValue.Key, out var fieldDeserializer))
            {
                if("null".Equals(fieldValue.Value, StringComparison.OrdinalIgnoreCase))
                    continue;// If the property is already null just continue. As calling functions like GetDouble,GetBoolValue do not process null.

                Debug.WriteLine($"found property {fieldValue.Key} to deserialize");
                fieldDeserializer.Invoke(new FormParseNode(fieldValue.Value)
                {
                    OnBeforeAssignFieldValues = OnBeforeAssignFieldValues,
                    OnAfterAssignFieldValues = OnAfterAssignFieldValues
                });
            }
            else if(itemAdditionalData != null)
            {
                Debug.WriteLine($"found additional property {fieldValue.Key} to deserialize");
                IDictionaryExtensions.TryAdd(itemAdditionalData, fieldValue.Key, fieldValue.Value);
            }
            else
            {
                Debug.WriteLine($"found additional property {fieldValue.Key} to deserialize but the model doesn't support additional data");
            }
        }
    }

    /// <inheritdoc/>
    public sbyte? GetSbyteValue() => sbyte.TryParse(DecodedValue, out var result) ? result : null;

    /// <inheritdoc/>
    public string GetStringValue() => DecodedValue;

    /// <inheritdoc/>
    public TimeSpan? GetTimeSpanValue()
    {
        var rawString = DecodedValue;
        if(string.IsNullOrEmpty(rawString))
            return null;

        // Parse an ISO8601 duration.http://en.wikipedia.org/wiki/ISO_8601#Durations to a TimeSpan
        return XmlConvert.ToTimeSpan(rawString);
    }

    /// <inheritdoc/>
    public Time? GetTimeValue() => DateTime.TryParse(DecodedValue, out var result) ? new Time(result) : null;

#if NET5_0_OR_GREATER
    IEnumerable<T?> IParseNode.GetCollectionOfEnumValues<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>()
#else
    IEnumerable<T?> IParseNode.GetCollectionOfEnumValues<T>()
#endif
    {
        foreach(var v in DecodedValue.Split(ComaSeparator, StringSplitOptions.RemoveEmptyEntries))
            yield return EnumHelpers.GetEnumValue<T>(v);
    }

#if NET5_0_OR_GREATER
    T? IParseNode.GetEnumValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>()
#else
    T? IParseNode.GetEnumValue<T>()
#endif
    {
        return EnumHelpers.GetEnumValue<T>(DecodedValue);
    }
}
