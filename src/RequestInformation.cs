// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Microsoft.Kiota.Abstractions
{
    /// <summary>
    ///     This class represents an abstract HTTP request.
    /// </summary>
    public class RequestInformation
    {
        /// <summary>
        /// Creates a new instance of <see cref="RequestInformation"/>.
        /// </summary>
        public RequestInformation()
        {

        }
        /// <summary>
        /// Creates a new instance of <see cref="RequestInformation"/> with the given method and url template.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="urlTemplate"></param>
        /// <param name="pathParameters"></param>
        public RequestInformation(Method method, string urlTemplate, IDictionary<string, object> pathParameters)
        {
            HttpMethod = method;
            UrlTemplate = urlTemplate;
            PathParameters = pathParameters;
        }
        /// <summary>
        /// Configures the current request configuration headers, query parameters, and options base on the callback provided.
        /// </summary>
        /// <typeparam name="T">Type for the query parameters</typeparam>
        /// <param name="requestConfiguration">Callback to configure the request</param>
        public void Configure<T>(Action<RequestConfiguration<T>>? requestConfiguration) where T : class, new()
        {
            if(requestConfiguration == null) return;
            var requestConfig = new RequestConfiguration<T>();
            requestConfiguration(requestConfig);
            AddQueryParameters(requestConfig.QueryParameters);
            AddRequestOptions(requestConfig.Options);
            AddHeaders(requestConfig.Headers);
        }
        internal const string RawUrlKey = "request-raw-url";
        private Uri? _rawUri;
        /// <summary>
        ///  The URI of the request.
        /// </summary>
        public Uri URI
        {
            set
            {
                if(value == null)
                    throw new ArgumentNullException(nameof(value));
                QueryParameters.Clear();
                PathParameters.Clear();
                _rawUri = value;
            }
            get
            {
                if(_rawUri != null)
                    return _rawUri;
                else if(PathParameters.TryGetValue(RawUrlKey, out var rawUrl) &&
                    rawUrl is string rawUrlString)
                {
                    URI = new Uri(rawUrlString);
                    return _rawUri!;
                }
                else
                {
                    if(UrlTemplate?.IndexOf("{+baseurl}", StringComparison.OrdinalIgnoreCase) >= 0 && !PathParameters.ContainsKey("baseurl"))
                        throw new InvalidOperationException($"{nameof(PathParameters)} must contain a value for \"baseurl\" for the url to be built.");

                    var substitutions = new Dictionary<string, object>();
                    foreach(var urlTemplateParameter in PathParameters)
                    {
                        substitutions.Add(urlTemplateParameter.Key, GetSanitizedValue(urlTemplateParameter.Value));
                    }

                    foreach(var queryStringParameter in QueryParameters)
                    {
                        if(queryStringParameter.Value != null)
                        {
                            substitutions.Add(queryStringParameter.Key, GetSanitizedValue(queryStringParameter.Value));
                        }
                    }

                    return new Uri(Std.UriTemplate.Expand(UrlTemplate, substitutions));
                }
            }
        }

        /// <summary>
        /// Sanitizes objects in order to appear appropiately in the URL
        /// </summary>
        /// <param name="value">Object to be sanitized</param>
        /// <returns>Sanitized object</returns>
        private static object GetSanitizedValue(object value) => value switch
        {
            bool boolean => boolean.ToString().ToLower(),// pass in a lowercase string as the final url will be uppercase due to the way ToString() works for booleans
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("o"),// Default to ISO 8601 for datetimeoffsets in the url.
            DateTime dateTime => dateTime.ToString("o"),// Default to ISO 8601 for datetimes in the url.
            Guid guid => guid.ToString("D"),// Default of 32 digits separated by hyphens
            Date date => date.ToString(), //Default to string format of the custom date object
            Time time => time.ToString(), //Default to string format of the custom time object
            _ => ReplaceEnumValueByStringRepresentation(value),//return object as is as the ToString method is good enough.
        };

        /// <summary>
        /// The Url template for the current request.
        /// </summary>
        public string? UrlTemplate { get; set; }
        /// <summary>
        /// The path parameters to use for the URL template when generating the URI.
        /// </summary>
        public IDictionary<string, object> PathParameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        ///  The <see cref="Method">HTTP method</see> of the request.
        /// </summary>
        public Method HttpMethod { get; set; }
        /// <summary>
        /// The Query Parameters of the request.
        /// </summary>
        public IDictionary<string, object> QueryParameters { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Vanity method to add the query parameters to the request query parameters dictionary.
        /// </summary>
        /// <param name="source">The query parameters to add.</param>
#if NET5_0_OR_GREATER
        public void AddQueryParameters<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T source)
#else
        public void AddQueryParameters<T>(T source)
#endif
        {
            if(source == null) return;
            foreach(var property in typeof(T)
                                        .GetProperties()
                                        .Select(
                                            x => (
                                                Name: x.GetCustomAttributes(false)
                                                    .OfType<QueryParameterAttribute>()
                                                    .FirstOrDefault()?.TemplateName ?? x.Name,
                                                Value: x.GetValue(source)
                                            )
                                        )
                                        .Where(x => x.Value != null &&
                                                    !QueryParameters.ContainsKey(x.Name!) &&
                                                    !string.IsNullOrEmpty(x.Value.ToString()) && // no need to add an empty string value
                                                    (x.Value is not ICollection collection || collection.Count > 0))) // no need to add empty collection
            {
                QueryParameters.AddOrReplace(property.Name!, ReplaceEnumValueByStringRepresentation(property.Value!));
            }
        }
        private static object ReplaceEnumValueByStringRepresentation(object source)
        {
            if(source is Enum enumValue && GetEnumName(enumValue) is string enumValueName)
            {
                return enumValueName;
            }
            else if(source is Array collection && collection.Length > 0 && collection.GetValue(0) is Enum)
            {
                var passedArray = new string[collection.Length];
                for(var i = 0; i < collection.Length; i++)
                {// this is ugly but necessary due to covariance limitations with pattern matching
                    passedArray[i] = GetEnumName((Enum)collection.GetValue(i)!)!;
                }
                return passedArray;
            }
            else return source;
        }
#if NET5_0_OR_GREATER
        private static string? GetEnumName<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T value) where T : Enum
#else
        private static string? GetEnumName<T>(T value) where T : Enum
#endif
        {
            var type = value.GetType();

            if(Enum.GetName(type, value) is not { } name)
                throw new ArgumentException($"Invalid Enum value {value} for enum of type {type}");

            if(type.GetMember(name).FirstOrDefault()?.GetCustomAttribute<EnumMemberAttribute>() is { } attribute)
                return attribute.Value;

            return name.ToFirstCharacterLowerCase();
        }
        /// <summary>
        /// The Request Headers.
        /// </summary>
        public RequestHeaders Headers { get; private set; } = new();
        /// <summary>
        /// Vanity method to add the headers to the request headers dictionary.
        /// </summary>
        public void AddHeaders(RequestHeaders headers)
        {
            if(headers == null) return;
            Headers.AddAll(headers);
        }
        /// <summary>
        /// The Request Body.
        /// </summary>
        public Stream Content { get; set; } = Stream.Null;
        private readonly Dictionary<string, IRequestOption> _requestOptions = new Dictionary<string, IRequestOption>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Gets the options for this request. Options are unique by type. If an option of the same type is added twice, the last one wins.
        /// </summary>
        public IEnumerable<IRequestOption> RequestOptions { get { return _requestOptions.Values; } }
        /// <summary>
        /// Adds an option to the request.
        /// </summary>
        /// <param name="options">The option to add.</param>
        public void AddRequestOptions(IEnumerable<IRequestOption> options)
        {
            if(options == null) return;
            foreach(var option in options.Where(x => x != null))
                _requestOptions.AddOrReplace(option.GetType().FullName!, option);
        }
        /// <summary>
        /// Removes given options from the current request.
        /// </summary>
        /// <param name="options">Options to remove.</param>
        public void RemoveRequestOptions(params IRequestOption[] options)
        {
            if(!options?.Any() ?? false) throw new ArgumentNullException(nameof(options));
            foreach(var optionName in options!.Where(x => x != null).Select(x => x.GetType().FullName))
                _requestOptions.Remove(optionName!);
        }

        /// <summary>
        /// Gets a <see cref="IRequestOption"/> instance of the matching type.
        /// </summary>
        public T? GetRequestOption<T>() => _requestOptions.TryGetValue(typeof(T).FullName!, out var requestOption) ? (T)requestOption : default;

        /// <summary>
        /// Adds a <see cref="IResponseHandler"/> as a <see cref="IRequestOption"/> for the request.
        /// </summary>
        public void SetResponseHandler(IResponseHandler responseHandler)
        {
            if(responseHandler == null)
                throw new ArgumentNullException(nameof(responseHandler));

            var responseHandlerOption = new ResponseHandlerOption
            {
                ResponseHandler = responseHandler
            };
            AddRequestOptions(new[] { responseHandlerOption });
        }

        private const string BinaryContentType = "application/octet-stream";
        private const string ContentTypeHeader = "Content-Type";
        /// <summary>
        /// Sets the request body to a binary stream.
        /// </summary>
        /// <param name="content">The binary stream to set as a body.</param>
        [Obsolete("Use SetStreamContent and pass the content type instead")]
        public void SetStreamContent(Stream content) => SetStreamContent(content, BinaryContentType);
        /// <summary>
        /// Sets the request body to a binary stream.
        /// </summary>
        /// <param name="content">The binary stream to set as a body.</param>
        /// <param name="contentType">The content type to set.</param>
        public void SetStreamContent(Stream content, string contentType)
        {
            using var activity = _activitySource?.StartActivity(nameof(SetStreamContent));
            SetRequestType(content, activity);
            Content = content;
            Headers.TryAdd(ContentTypeHeader, contentType);
        }
        private static ActivitySource _activitySource = new(typeof(RequestInformation).Namespace!);
        /// <summary>
        /// Sets the request body from a model with the specified content type.
        /// </summary>
        /// <param name="requestAdapter">The core service to get the serialization writer from.</param>
        /// <param name="items">The models to serialize.</param>
        /// <param name="contentType">The content type to set.</param>
        /// <typeparam name="T">The model type to serialize.</typeparam>
        public void SetContentFromParsable<T>(IRequestAdapter requestAdapter, string contentType, IEnumerable<T> items) where T : IParsable
        {
            using var activity = _activitySource?.StartActivity(nameof(SetContentFromParsable));
            using var writer = GetSerializationWriter(requestAdapter, contentType, items);
            SetRequestType(items.FirstOrDefault(static x => x != null), activity);
            writer.WriteCollectionOfObjectValues(null, items);
            Headers.TryAdd(ContentTypeHeader, contentType);
            Content = writer.GetSerializedContent();
        }
        /// <summary>
        /// Sets the request body from a model with the specified content type.
        /// </summary>
        /// <param name="requestAdapter">The core service to get the serialization writer from.</param>
        /// <param name="item">The model to serialize.</param>
        /// <param name="contentType">The content type to set.</param>
        /// <typeparam name="T">The model type to serialize.</typeparam>
        public void SetContentFromParsable<T>(IRequestAdapter requestAdapter, string contentType, T item) where T : IParsable
        {
            using var activity = _activitySource?.StartActivity(nameof(SetContentFromParsable));
            using var writer = GetSerializationWriter(requestAdapter, contentType, item);
            SetRequestType(item, activity);
            if(item is MultipartBody mpBody)
            {
                contentType += "; boundary=" + mpBody.Boundary;
                mpBody.RequestAdapter = requestAdapter;
            }
            writer.WriteObjectValue(null, item);
            Headers.TryAdd(ContentTypeHeader, contentType);
            Content = writer.GetSerializedContent();
        }
        private static void SetRequestType(object? result, Activity? activity)
        {
            if(activity == null) return;
            if(result == null) return;
            activity.SetTag("com.microsoft.kiota.request.type", result.GetType().FullName);
        }
        private static ISerializationWriter GetSerializationWriter<T>(IRequestAdapter requestAdapter, string contentType, T item)
        {
            if(string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
            if(requestAdapter == null) throw new ArgumentNullException(nameof(requestAdapter));
            if(item == null) throw new InvalidOperationException($"{nameof(item)} cannot be null");
            return requestAdapter.SerializationWriterFactory.GetSerializationWriter(contentType);
        }
        /// <summary>
        /// Sets the request body from a scalar value with the specified content type.
        /// </summary>
        /// <param name="requestAdapter">The core service to get the serialization writer from.</param>
        /// <param name="items">The scalar values to serialize.</param>
        /// <param name="contentType">The content type to set.</param>
        /// <typeparam name="T">The model type to serialize.</typeparam>
        public void SetContentFromScalarCollection<T>(IRequestAdapter requestAdapter, string contentType, IEnumerable<T> items)
        {
            using var activity = _activitySource?.StartActivity(nameof(SetContentFromScalarCollection));
            using var writer = GetSerializationWriter(requestAdapter, contentType, items);
            SetRequestType(items.FirstOrDefault(static x => x != null), activity);
            writer.WriteCollectionOfPrimitiveValues(null, items);
            Headers.TryAdd(ContentTypeHeader, contentType);
            Content = writer.GetSerializedContent();
        }
        /// <summary>
        /// Sets the request body from a scalar value with the specified content type.
        /// </summary>
        /// <param name="requestAdapter">The core service to get the serialization writer from.</param>
        /// <param name="item">The scalar value to serialize.</param>
        /// <param name="contentType">The content type to set.</param>
        /// <typeparam name="T">The model type to serialize.</typeparam>
        public void SetContentFromScalar<T>(IRequestAdapter requestAdapter, string contentType, T item)
        {
            using var activity = _activitySource?.StartActivity(nameof(SetContentFromScalar));
            using var writer = GetSerializationWriter(requestAdapter, contentType, item);
            SetRequestType(item, activity);
            switch(item)
            {
                case string s:
                    writer.WriteStringValue(null, s);
                    break;
                case bool b:
                    writer.WriteBoolValue(null, b);
                    break;
                case byte b:
                    writer.WriteByteValue(null, b);
                    break;
                case sbyte b:
                    writer.WriteSbyteValue(null, b);
                    break;
                case int i:
                    writer.WriteIntValue(null, i);
                    break;
                case float f:
                    writer.WriteFloatValue(null, f);
                    break;
                case long l:
                    writer.WriteLongValue(null, l);
                    break;
                case double d:
                    writer.WriteDoubleValue(null, d);
                    break;
                case Guid g:
                    writer.WriteGuidValue(null, g);
                    break;
                case DateTimeOffset dto:
                    writer.WriteDateTimeOffsetValue(null, dto);
                    break;
                case TimeSpan timeSpan:
                    writer.WriteTimeSpanValue(null, timeSpan);
                    break;
                case Date date:
                    writer.WriteDateValue(null, date);
                    break;
                case Time time:
                    writer.WriteTimeValue(null, time);
                    break;
                case null:
                    writer.WriteNullValue(null);
                    break;
                default:
                    throw new InvalidOperationException($"error serialization data value with unknown type {item?.GetType()}");
            }
            Headers.TryAdd(ContentTypeHeader, contentType);
            Content = writer.GetSerializedContent();
        }
    }
}
