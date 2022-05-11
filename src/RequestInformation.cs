// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
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
        private Uri _rawUri;
        /// <summary>
        ///  The URI of the request.
        /// </summary>
        public Uri URI {
            set {
                if(value == null)
                    throw new ArgumentNullException(nameof(value));
                QueryParameters.Clear();
                PathParameters.Clear();
                _rawUri = value;
            }
            get {
                if(_rawUri != null)
                    return _rawUri;
                else if(PathParameters.TryGetValue("request-raw-url", out var rawUrl) &&
                    rawUrl is string rawUrlString) {
                    URI = new Uri(rawUrlString);
                    return _rawUri;
                }
                else
                {
                    var parsedUrlTemplate = new UriTemplate(UrlTemplate);
                    foreach(var urlTemplateParameter in PathParameters)
                    {
                        // if the value is boolean, lets pass in a lowercase string as the final url will be uppercase due to the way ToString() works for booleans
                        var sanitizedValue = (urlTemplateParameter.Value is bool boolValue) ? boolValue.ToString().ToLower() : urlTemplateParameter.Value;
                        parsedUrlTemplate.SetParameter(urlTemplateParameter.Key, sanitizedValue);
                    }

                    foreach(var queryStringParameter in QueryParameters)
                        if(queryStringParameter.Value != null)
                        {
                            // if the value is boolean, lets pass in a lowercase string as the final url will be uppercase due to the way ToString() works for booleans
                            var sanitizedValue = (queryStringParameter.Value is bool boolValue) ? boolValue.ToString().ToLower() : queryStringParameter.Value;
                            parsedUrlTemplate.SetParameter(queryStringParameter.Key, sanitizedValue);
                        }
                    return new Uri(parsedUrlTemplate.Resolve());
                }
            }
        }
        /// <summary>
        /// The Url template for the current request.
        /// </summary>
        public string UrlTemplate { get; set; }
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
                                        .Where(x => x.Value != null && !QueryParameters.ContainsKey(x.Name)))
            {
                QueryParameters.AddOrReplace(property.Name, property.Value);
            }
        }
        /// <summary>
        /// The Request Headers.
        /// </summary>
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Adds request headers to the request.
        /// </summary>
        /// <param name="source">The request headers to add.</param>
        public void AddHeaders(IDictionary<string, string> source)
        {
            if(source == null) return;
            foreach(var header in source)
                Headers.AddOrReplace(header.Key, header.Value);
        }
        /// <summary>
        /// The Request Body.
        /// </summary>
        public Stream Content { get; set; }
        private Dictionary<string, IRequestOption> _requestOptions = new Dictionary<string, IRequestOption>(StringComparer.OrdinalIgnoreCase);
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
                _requestOptions.AddOrReplace(option.GetType().FullName, option);
        }
        /// <summary>
        /// Removes given options from the current request.
        /// </summary>
        /// <param name="options">Options to remove.</param>
        public void RemoveRequestOptions(params IRequestOption[] options)
        {
            if(!options?.Any() ?? false) throw new ArgumentNullException(nameof(options));
            foreach(var optionName in options.Where(x => x != null).Select(x => x.GetType().FullName))
                _requestOptions.Remove(optionName);
        }
        private const string BinaryContentType = "application/octet-stream";
        private const string ContentTypeHeader = "Content-Type";
        /// <summary>
        /// Sets the request body to a binary stream.
        /// </summary>
        /// <param name="content">The binary stream to set as a body.</param>
        public void SetStreamContent(Stream content)
        {
            Content = content;
            Headers.Add(ContentTypeHeader, BinaryContentType);
        }
        /// <summary>
        /// Sets the request body from a model with the specified content type.
        /// </summary>
        /// <param name="requestAdapter">The core service to get the serialization writer from.</param>
        /// <param name="items">The models to serialize.</param>
        /// <param name="contentType">The content type to set.</param>
        /// <typeparam name="T">The model type to serialize.</typeparam>
        public void SetContentFromParsable<T>(IRequestAdapter requestAdapter, string contentType, params T[] items) where T : IParsable
        {
            using var writer = getSerializationWriter(requestAdapter, contentType, items);
            if(items.Count() == 1)
                writer.WriteObjectValue(null, items[0]);
            else
                writer.WriteCollectionOfObjectValues(null, items);
            Headers.Add(ContentTypeHeader, contentType);
            Content = writer.GetSerializedContent();
        }
        private ISerializationWriter getSerializationWriter<T>(IRequestAdapter requestAdapter, string contentType, params T[] items)
        {
            if(string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
            if(requestAdapter == null) throw new ArgumentNullException(nameof(requestAdapter));
            if(items == null || !items.Any()) throw new InvalidOperationException($"{nameof(items)} cannot be null or empty");
            return requestAdapter.SerializationWriterFactory.GetSerializationWriter(contentType);
        }
        /// <summary>
        /// Sets the request body from a scalar value with the specified content type.
        /// </summary>
        /// <param name="requestAdapter">The core service to get the serialization writer from.</param>
        /// <param name="items">The scalar values to serialize.</param>
        /// <param name="contentType">The content type to set.</param>
        /// <typeparam name="T">The model type to serialize.</typeparam>
        public void SetContentFromScalar<T>(IRequestAdapter requestAdapter, string contentType, params T[] items)
        {
            using var writer = getSerializationWriter(requestAdapter, contentType, items);
            if(items.Count() == 1)
                switch(items[0])
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
                        throw new InvalidOperationException($"error serialization data value with unknown type {items[0]?.GetType()}");
                }
            else
                writer.WriteCollectionOfPrimitiveValues(null, items);
            Headers.Add(ContentTypeHeader, contentType);
            Content = writer.GetSerializedContent();
        }
    }
}
