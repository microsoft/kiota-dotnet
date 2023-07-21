// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using Tavis.UriTemplates;

namespace Microsoft.Kiota.Abstractions
{
    /// <summary>
    ///     This class represents an abstract HTTP request.
    /// </summary>
    public class RequestInformation
    {
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

                    var parsedUrlTemplate = new UriTemplate(UrlTemplate, caseInsensitiveParameterNames: true);
                    foreach(var urlTemplateParameter in PathParameters)
                    {
                        parsedUrlTemplate.SetParameter(urlTemplateParameter.Key, GetSanitizedValue(urlTemplateParameter.Value));
                    }

                    foreach(var queryStringParameter in QueryParameters)
                    {
                        if(queryStringParameter.Value != null)
                        {
                            parsedUrlTemplate.SetParameter(queryStringParameter.Key, GetSanitizedValue(queryStringParameter.Value));
                        }
                    }
                    return new Uri(parsedUrlTemplate.Resolve());
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
            _ => value,//return object as is as the ToString method is good enough.
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
        public void AddQueryParameters(object source)
        {
            if(source == null) return;
            foreach(var property in source.GetType()
                                        .GetProperties()
                                        .Select(
                                            x => (
                                                Name: x.GetCustomAttributes(false)
                                                    .OfType<QueryParameterAttribute>()
                                                    .FirstOrDefault()?.TemplateName ?? x.Name.ToFirstCharacterLowerCase(),
                                                Value: x.GetValue(source)
                                            )
                                        )
                                        .Where(x => x.Value != null &&
                                                    !QueryParameters.ContainsKey(x.Name!) &&
                                                    !string.IsNullOrEmpty(x.Value.ToString()) && // no need to add an empty string value
                                                    (x.Value is not ICollection collection || collection.Count > 0))) // no need to add empty collection
            {
                QueryParameters.AddOrReplace(property.Name!, property.Value!);
            }
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
        public void SetStreamContent(Stream content)
        {
            using var activity = _activitySource?.StartActivity(nameof(SetStreamContent));
            SetRequestType(content, activity);
            Content = content;
            Headers.Add(ContentTypeHeader, BinaryContentType);
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
            Headers.Add(ContentTypeHeader, contentType);
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
            writer.WriteObjectValue(null, item);
            if(item is MultipartBody mpBody)
                contentType += "; boundary=" + mpBody.Boundary;
            Headers.Add(ContentTypeHeader, contentType);
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
            Headers.Add(ContentTypeHeader, contentType);
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
            Headers.Add(ContentTypeHeader, contentType);
            Content = writer.GetSerializedContent();
        }
    }
}
