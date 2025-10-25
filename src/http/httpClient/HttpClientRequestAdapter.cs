// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.IO;
using System.Net;
using System.Net.Http;
#if NET5_0_OR_GREATER
using System.Net.Http.Headers;
#endif
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Helpers;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

namespace Microsoft.Kiota.Http.HttpClientLibrary
{
    /// <summary>
    /// The <see cref="IRequestAdapter"/> implementation for sending requests.
    /// </summary>
    public class HttpClientRequestAdapter : IRequestAdapter, IDisposable
    {
        private readonly HttpClient client;
        private readonly IAuthenticationProvider authProvider;
        private IParseNodeFactory pNodeFactory;
        private ISerializationWriterFactory sWriterFactory;
        private string? baseUrl;
        private readonly bool createdClient;
        private readonly ObservabilityOptions obsOptions;
        private readonly ActivitySource activitySource;
#if !NET5_0_OR_GREATER
        private readonly Version httpVersion;
#endif
#if NETSTANDARD2_1_OR_GREATER
        private static readonly Version defaultHttpVersion = HttpVersion.Version20;
#elif NETSTANDARD2_0 || NETFRAMEWORK
        private static readonly Version defaultHttpVersion = HttpVersion.Version11;
#endif
#if NET5_0_OR_GREATER
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientRequestAdapter"/> class.
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="parseNodeFactory">The parse node factory.</param>
        /// <param name="serializationWriterFactory">The serialization writer factory.</param>
        /// <param name="httpClient">The native HTTP client.</param>
        /// <param name="observabilityOptions">The observability options.</param>
        /// </summary>
        public HttpClientRequestAdapter(IAuthenticationProvider authenticationProvider, IParseNodeFactory? parseNodeFactory = null, ISerializationWriterFactory? serializationWriterFactory = null, HttpClient? httpClient = null, ObservabilityOptions? observabilityOptions = null)
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientRequestAdapter"/> class.
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="parseNodeFactory">The parse node factory.</param>
        /// <param name="serializationWriterFactory">The serialization writer factory.</param>
        /// <param name="httpClient">The native HTTP client.</param>
        /// <param name="observabilityOptions">The observability options.</param>
        /// </summary>
        public HttpClientRequestAdapter(IAuthenticationProvider authenticationProvider, IParseNodeFactory? parseNodeFactory = null, ISerializationWriterFactory? serializationWriterFactory = null, HttpClient? httpClient = null, ObservabilityOptions? observabilityOptions = null):
            this(authenticationProvider, parseNodeFactory, serializationWriterFactory, httpClient, observabilityOptions, null);

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientRequestAdapter"/> class.
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="parseNodeFactory">The parse node factory.</param>
        /// <param name="serializationWriterFactory">The serialization writer factory.</param>
        /// <param name="httpClient">The native HTTP client.</param>
        /// <param name="observabilityOptions">The observability options.</param>
        /// <param name="httpVersion">The HTTP version.</param>
        /// </summary>
        public HttpClientRequestAdapter(IAuthenticationProvider authenticationProvider, IParseNodeFactory? parseNodeFactory = null, ISerializationWriterFactory? serializationWriterFactory = null, HttpClient? httpClient = null, ObservabilityOptions? observabilityOptions = null, Version? httpVersion = null)
#endif
        {
            authProvider = authenticationProvider ?? throw new ArgumentNullException(nameof(authenticationProvider));
            createdClient = httpClient == null;
            client = httpClient ?? KiotaClientFactory.Create();
            BaseUrl = client.BaseAddress?.ToString();
            pNodeFactory = parseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance;
            sWriterFactory = serializationWriterFactory ?? SerializationWriterFactoryRegistry.DefaultInstance;
            obsOptions = observabilityOptions ?? new ObservabilityOptions();
            activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
#if !NET5_0_OR_GREATER
            this.httpVersion = httpVersion ?? defaultHttpVersion;
#endif
        }
        /// <summary>Factory to use to get a serializer for payload serialization</summary>
        public ISerializationWriterFactory SerializationWriterFactory
        {
            get
            {
                return sWriterFactory;
            }
        }
        /// <summary>
        /// The base url for every request.
        /// </summary>
        public string? BaseUrl
        {
            get => baseUrl;
            set => this.baseUrl = value?.TrimEnd('/');
        }
        private static readonly char[] charactersToDecodeForUriTemplate = ['$', '.', '-', '~'];
        private static readonly Regex queryParametersCleanupRegex = new(@"\{\?[^\}]+}", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));
        private Activity? startTracingSpan(RequestInformation requestInfo, string methodName)
        {
            var decodedUriTemplate = ParametersNameDecodingHandler.DecodeUriEncodedString(requestInfo.UrlTemplate, charactersToDecodeForUriTemplate);
            var telemetryPathValue = string.IsNullOrEmpty(decodedUriTemplate) ? string.Empty : queryParametersCleanupRegex.Replace(decodedUriTemplate, string.Empty);
            var span = activitySource?.StartActivity($"{methodName} - {telemetryPathValue}");
            span?.SetTag("url.uri_template", decodedUriTemplate);
            return span;
        }
        /// <summary>
        /// Send a <see cref="RequestInformation"/> instance with a collection instance of <typeparam name="ModelType"></typeparam>
        /// </summary>
        /// <param name="requestInfo">The <see cref="RequestInformation"/> instance to send</param>
        /// <param name="factory">The factory of the response model to deserialize the response into.</param>
        /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the request.</param>
        public async Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(RequestInformation requestInfo, ParsableFactory<ModelType> factory, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default) where ModelType : IParsable
        {
            using var span = startTracingSpan(requestInfo, nameof(SendCollectionAsync));
            var response = await GetHttpResponseMessageAsync(requestInfo, cancellationToken, span).ConfigureAwait(false);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            if(requestInfo.Content is not null)
                await requestInfo.Content.DisposeAsync().ConfigureAwait(false);
#else
            requestInfo.Content?.Dispose();
#endif
            var responseHandler = GetResponseHandler(requestInfo);
            if(responseHandler == null)
            {
                try
                {
                    await ThrowIfFailedResponseAsync(response, errorMapping, span, cancellationToken).ConfigureAwait(false);
                    if(shouldReturnNull(response)) return default;
                    var rootNode = await GetRootParseNodeAsync(response, cancellationToken).ConfigureAwait(false);
                    using var spanForDeserialization = activitySource?.StartActivity(nameof(IParseNode.GetCollectionOfObjectValues));
                    var result = rootNode?.GetCollectionOfObjectValues<ModelType>(factory);
                    SetResponseType(result, span);
                    return result;
                }
                finally
                {
                    await DrainAsync(response, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                span?.AddEvent(new ActivityEvent(EventResponseHandlerInvokedKey));
                return await responseHandler.HandleResponseAsync<HttpResponseMessage, IEnumerable<ModelType>>(response, errorMapping).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Executes the HTTP request specified by the given RequestInformation and returns the deserialized primitive response model collection.
        /// </summary>
        /// <param name="requestInfo">The RequestInformation object to use for the HTTP request.</param>
        /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the request.</param>
        /// <returns>The deserialized primitive response model collection.</returns>
#if NET5_0_OR_GREATER
        public async Task<IEnumerable<ModelType>?> SendPrimitiveCollectionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] ModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default)
#else
        public async Task<IEnumerable<ModelType>?> SendPrimitiveCollectionAsync<ModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default)
#endif
        {
            using var span = startTracingSpan(requestInfo, nameof(SendPrimitiveCollectionAsync));
            var response = await GetHttpResponseMessageAsync(requestInfo, cancellationToken, span).ConfigureAwait(false);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            if(requestInfo.Content is not null)
                await requestInfo.Content.DisposeAsync().ConfigureAwait(false);
#else
            requestInfo.Content?.Dispose();
#endif
            var responseHandler = GetResponseHandler(requestInfo);
            if(responseHandler == null)
            {
                try
                {
                    await ThrowIfFailedResponseAsync(response, errorMapping, span, cancellationToken).ConfigureAwait(false);
                    if(shouldReturnNull(response)) return default;
                    var rootNode = await GetRootParseNodeAsync(response, cancellationToken).ConfigureAwait(false);
                    using var spanForDeserialization = activitySource?.StartActivity(nameof(IParseNode.GetCollectionOfPrimitiveValues));
                    var result = rootNode?.GetCollectionOfPrimitiveValues<ModelType>();
                    SetResponseType(result, span);
                    return result;
                }
                finally
                {
                    await DrainAsync(response, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                span?.AddEvent(new ActivityEvent(EventResponseHandlerInvokedKey));
                return await responseHandler.HandleResponseAsync<HttpResponseMessage, IEnumerable<ModelType>>(response, errorMapping).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// The key for the tracing event raised when a response handler is called.
        /// </summary>
        public const string EventResponseHandlerInvokedKey = "com.microsoft.kiota.response_handler_invoked";
        /// <summary>
        /// Send a <see cref="RequestInformation"/> instance with an instance of <typeparam name="ModelType"></typeparam>
        /// </summary>
        /// <param name="requestInfo">The <see cref="RequestInformation"/> instance to send</param>
        /// <param name="factory">The factory of the response model to deserialize the response into.</param>
        /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the request.</param>
        /// <returns>The deserialized response model.</returns>
        public async Task<ModelType?> SendAsync<ModelType>(RequestInformation requestInfo, ParsableFactory<ModelType> factory, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default) where ModelType : IParsable
        {
            using var span = startTracingSpan(requestInfo, nameof(SendAsync));
            var response = await GetHttpResponseMessageAsync(requestInfo, cancellationToken, span).ConfigureAwait(false);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            if(requestInfo.Content is not null)
                await requestInfo.Content.DisposeAsync().ConfigureAwait(false);
#else
            requestInfo.Content?.Dispose();
#endif
            var responseHandler = GetResponseHandler(requestInfo);
            if(responseHandler == null)
            {
                try
                {
                    await ThrowIfFailedResponseAsync(response, errorMapping, span, cancellationToken).ConfigureAwait(false);
                    if(shouldReturnNull(response)) return default;
                    var rootNode = await GetRootParseNodeAsync(response, cancellationToken).ConfigureAwait(false);
                    if(rootNode == null) return default;
                    using var spanForDeserialization = activitySource?.StartActivity(nameof(IParseNode.GetObjectValue));
                    var result = rootNode.GetObjectValue<ModelType>(factory);
                    SetResponseType(result, span);
                    return result;
                }
                finally
                {
                    if(typeof(ModelType) != typeof(Stream))
                    {
                        await DrainAsync(response, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                span?.AddEvent(new ActivityEvent(EventResponseHandlerInvokedKey));
                return await responseHandler.HandleResponseAsync<HttpResponseMessage, ModelType>(response, errorMapping).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Send a <see cref="RequestInformation"/> instance with a primitive instance of <typeparam name="ModelType"></typeparam>
        /// </summary>
        /// <param name="requestInfo">The <see cref="RequestInformation"/> instance to send</param>
        /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the request.</param>
        /// <returns>The deserialized primitive response model.</returns>
#if NET5_0_OR_GREATER
        public async Task<ModelType?> SendPrimitiveAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] ModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default)
#else
        public async Task<ModelType?> SendPrimitiveAsync<ModelType>(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default)
#endif
        {
            using var span = startTracingSpan(requestInfo, nameof(SendPrimitiveAsync));
            var modelType = typeof(ModelType);
            var isStreamResponse = modelType == typeof(Stream);
            var response = await GetHttpResponseMessageAsync(requestInfo, cancellationToken, span, isStreamResponse: isStreamResponse).ConfigureAwait(false);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            if(requestInfo.Content is not null)
                await requestInfo.Content.DisposeAsync().ConfigureAwait(false);
#else
            requestInfo.Content?.Dispose();
#endif
            var responseHandler = GetResponseHandler(requestInfo);
            if(responseHandler == null)
            {
                try
                {
                    await ThrowIfFailedResponseAsync(response, errorMapping, span, cancellationToken).ConfigureAwait(false);
                    if(shouldReturnNull(response)) return default;
                    if(isStreamResponse)
                    {
#if NET5_0_OR_GREATER
                        var result = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
                        var result = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
                        if(result.CanSeek && result.Length == 0)
                        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
                            await result.DisposeAsync().ConfigureAwait(false);
#else
                            result.Dispose();
#endif
                            return default;
                        }
                        SetResponseType(result, span);
                        return (ModelType)(result as object);
                    }
                    else
                    {
                        var rootNode = await GetRootParseNodeAsync(response, cancellationToken).ConfigureAwait(false);
                        object? result;
                        using var spanForDeserialization = activitySource?.StartActivity($"Get{modelType.Name.TrimEnd('?')}Value");
                        if(rootNode == null)
                        {
                            result = null;
                        }
                        else if(modelType == typeof(bool?))
                        {
                            result = rootNode.GetBoolValue();
                        }
                        else if(modelType == typeof(byte?))
                        {
                            result = rootNode.GetByteValue();
                        }
                        else if(modelType == typeof(sbyte?))
                        {
                            result = rootNode.GetSbyteValue();
                        }
                        else if(modelType == typeof(string))
                        {
                            result = rootNode.GetStringValue();
                        }
                        else if(modelType == typeof(int?))
                        {
                            result = rootNode.GetIntValue();
                        }
                        else if(modelType == typeof(float?))
                        {
                            result = rootNode.GetFloatValue();
                        }
                        else if(modelType == typeof(long?))
                        {
                            result = rootNode.GetLongValue();
                        }
                        else if(modelType == typeof(double?))
                        {
                            result = rootNode.GetDoubleValue();
                        }
                        else if(modelType == typeof(decimal?))
                        {
                            result = rootNode.GetDecimalValue();
                        }
                        else if(modelType == typeof(Guid?))
                        {
                            result = rootNode.GetGuidValue();
                        }
                        else if(modelType == typeof(DateTimeOffset?))
                        {
                            result = rootNode.GetDateTimeOffsetValue();
                        }
                        else if(modelType == typeof(TimeSpan?))
                        {
                            result = rootNode.GetTimeSpanValue();
                        }
                        else if(modelType == typeof(Date?))
                        {
                            result = rootNode.GetDateValue();
                        }
                        else if(rootNode.GetStringValue() is { Length: > 0 } rawValue)
                        {
                            result = EnumHelpers.GetEnumValue(modelType, rawValue);
                        }
                        else throw new InvalidOperationException("error handling the response, unexpected type");
                        SetResponseType(result, span);
                        return (ModelType)result!;
                    }
                }
                finally
                {
                    if(typeof(ModelType) != typeof(Stream))
                    {
                        await DrainAsync(response, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                span?.AddEvent(new ActivityEvent(EventResponseHandlerInvokedKey));
                return await responseHandler.HandleResponseAsync<HttpResponseMessage, ModelType>(response, errorMapping).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Send a <see cref="RequestInformation"/> instance with an empty request body
        /// </summary>
        /// <param name="requestInfo">The <see cref="RequestInformation"/> instance to send</param>
        /// <param name="errorMapping">The error factories mapping to use in case of a failed request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancelling the request.</param>
        /// <returns></returns>
        public async Task SendNoContentAsync(RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default, CancellationToken cancellationToken = default)
        {
            using var span = startTracingSpan(requestInfo, nameof(SendNoContentAsync));
            var response = await GetHttpResponseMessageAsync(requestInfo, cancellationToken, span).ConfigureAwait(false);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            if(requestInfo.Content is not null)
                await requestInfo.Content.DisposeAsync().ConfigureAwait(false);
#else
            requestInfo.Content?.Dispose();
#endif
            var responseHandler = GetResponseHandler(requestInfo);
            if(responseHandler == null)
            {
                try
                {
                    await ThrowIfFailedResponseAsync(response, errorMapping, span, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    await DrainAsync(response, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                span?.AddEvent(new ActivityEvent(EventResponseHandlerInvokedKey));
                await responseHandler.HandleResponseAsync<HttpResponseMessage, object>(response, errorMapping).ConfigureAwait(false);
            }
        }
        private static void SetResponseType(object? result, Activity? activity)
        {
            if(result != null)
            {
                activity?.SetTag("com.microsoft.kiota.response.type", result.GetType().FullName);
            }
        }
        private static async Task DrainAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if(response.Content != null)
            {
#if NET5_0_OR_GREATER
                using var discard = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
                using var discard = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
                response.Content.Dispose();
            }
            response.Dispose();
        }
        private static bool shouldReturnNull(HttpResponseMessage response)
        {
            return response.StatusCode == HttpStatusCode.NoContent
                   || response.Content == null
                   || response.Content.GetType().Name.Equals("EmptyContent", StringComparison.OrdinalIgnoreCase);// In NET 5 and above, Content is never null but represented by the internal class EmptyContent
                                                                                                                 // which MAY return instances of EmptyReadStream on reading(which is not seekable thus we can't read/get the length)
                                                                                                                 // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http/src/System/Net/Http/EmptyReadStream.cs
        }
        /// <summary>
        /// The attribute name used to indicate whether an error code mapping was found.
        /// </summary>
        public const string ErrorMappingFoundAttributeName = "com.microsoft.kiota.error.mapping_found";
        /// <summary>
        /// The attribute name used to indicate whether the error response contained a body.
        /// </summary>
        public const string ErrorBodyFoundAttributeName = "com.microsoft.kiota.error.body_found";
        private async Task ThrowIfFailedResponseAsync(HttpResponseMessage response, Dictionary<string, ParsableFactory<IParsable>>? errorMapping, Activity? activityForAttributes, CancellationToken cancellationToken)
        {
            using var span = activitySource?.StartActivity(nameof(ThrowIfFailedResponseAsync));
            if(response.IsSuccessStatusCode || response.StatusCode is HttpStatusCode.Moved or HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.Redirect) return;

            activityForAttributes?.SetStatus(ActivityStatusCode.Error, "received_error_response");

            var statusCodeAsInt = (int)response.StatusCode;
            var statusCodeAsString = statusCodeAsInt.ToString();
            var responseHeadersDictionary = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
            foreach(var header in response.Headers)
                responseHeadersDictionary[header.Key] = header.Value;
            ParsableFactory<IParsable>? errorFactory;
            if(errorMapping == null ||
                !errorMapping.TryGetValue(statusCodeAsString, out errorFactory) &&
                !(statusCodeAsInt >= 400 && statusCodeAsInt < 500 && errorMapping.TryGetValue("4XX", out errorFactory)) &&
                !(statusCodeAsInt >= 500 && statusCodeAsInt < 600 && errorMapping.TryGetValue("5XX", out errorFactory)) &&
                !errorMapping.TryGetValue("XXX", out errorFactory))
            {
                activityForAttributes?.SetTag(ErrorMappingFoundAttributeName, false);
                throw new ApiException($"The server returned an unexpected status code and no error factory is registered for this code: {statusCodeAsString}")
                {
                    ResponseStatusCode = statusCodeAsInt,
                    ResponseHeaders = responseHeadersDictionary
                };
            }
            activityForAttributes?.SetTag(ErrorMappingFoundAttributeName, true);

            var rootNode = await GetRootParseNodeAsync(response, cancellationToken).ConfigureAwait(false);
            activityForAttributes?.SetTag(ErrorBodyFoundAttributeName, rootNode != null);
            var spanForDeserialization = activitySource?.StartActivity(nameof(IParseNode.GetObjectValue));
            var result = rootNode?.GetObjectValue(errorFactory);
            SetResponseType(result, activityForAttributes);
            spanForDeserialization?.Dispose();
            if(result is not Exception ex)
                throw new ApiException($"The server returned an unexpected status code and the error registered for this code failed to deserialize: {statusCodeAsString}")
                {
                    ResponseStatusCode = statusCodeAsInt,
                    ResponseHeaders = responseHeadersDictionary
                };
            if(result is ApiException apiEx)
            {
                apiEx.ResponseStatusCode = statusCodeAsInt;
                apiEx.ResponseHeaders = responseHeadersDictionary;
            }

            throw ex;
        }
        private static IResponseHandler? GetResponseHandler(RequestInformation requestInfo)
        {
            return requestInfo.GetRequestOption<ResponseHandlerOption>()?.ResponseHandler;
        }
        private async Task<IParseNode?> GetRootParseNodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            using var span = activitySource?.StartActivity(nameof(GetRootParseNodeAsync));
            var responseContentType = response.Content?.Headers?.ContentType?.MediaType?.ToLowerInvariant();
            if(string.IsNullOrEmpty(responseContentType))
                return null;
#if NET5_0_OR_GREATER
            using var contentStream = await (response.Content?.ReadAsStreamAsync(cancellationToken) ?? Task.FromResult(Stream.Null)).ConfigureAwait(false);
#else
            using var contentStream = await (response.Content?.ReadAsStreamAsync() ?? Task.FromResult(Stream.Null)).ConfigureAwait(false);
#endif
            if(contentStream == Stream.Null || (contentStream.CanSeek && contentStream.Length == 0))
                return null;// ensure a useful stream is passed to the factory
#pragma warning disable CS0618 // Type or member is obsolete
            //TODO remove with v2
            var rootNode = pNodeFactory is IAsyncParseNodeFactory asyncParseNodeFactory ? await asyncParseNodeFactory.GetRootParseNodeAsync(responseContentType!, contentStream, cancellationToken).ConfigureAwait(false) : pNodeFactory.GetRootParseNode(responseContentType!, contentStream);
#pragma warning restore CS0618 // Type or member is obsolete
            return rootNode;
        }
        private const string ClaimsKey = "claims";
        private const string BearerAuthenticationScheme = "Bearer";
        private async Task<HttpResponseMessage> GetHttpResponseMessageAsync(RequestInformation requestInfo, CancellationToken cancellationToken, Activity? activityForAttributes, string? claims = default, bool isStreamResponse = false)
        {
            using var span = activitySource?.StartActivity(nameof(GetHttpResponseMessageAsync));
            if(requestInfo == null)
                throw new ArgumentNullException(nameof(requestInfo));

            SetBaseUrlForRequestInformation(requestInfo);

            var additionalAuthenticationContext = string.IsNullOrEmpty(claims) ? null : new Dictionary<string, object> { { ClaimsKey, claims! } };
            await authProvider.AuthenticateRequestAsync(requestInfo, additionalAuthenticationContext, cancellationToken).ConfigureAwait(false);

            using var message = GetRequestMessageFromRequestInformation(requestInfo, activityForAttributes);
            var response = isStreamResponse ? await this.client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false) :
                                                await this.client.SendAsync(message, cancellationToken).ConfigureAwait(false);
            if(response == null)
            {
                var ex = new InvalidOperationException("Could not get a response after calling the service");
                throw ex;
            }
            if(response.Headers.TryGetValues("Content-Length", out var contentLengthValues))
            {
                using var contentLengthEnumerator = contentLengthValues.GetEnumerator();
                if(contentLengthEnumerator.MoveNext() && int.TryParse(contentLengthEnumerator.Current, out var contentLength))
                {
                    activityForAttributes?.SetTag("http.response.body.size", contentLength);
                }
            }
            if(response.Headers.TryGetValues("Content-Type", out var contentTypeValues))
            {
                using var contentTypeEnumerator = contentTypeValues.GetEnumerator();
                if(contentTypeEnumerator.MoveNext())
                {
                    activityForAttributes?.SetTag("http.response.header.content-type", contentTypeEnumerator.Current);
                }
            }
            activityForAttributes?.SetTag("http.response.status_code", (int)response.StatusCode);
            activityForAttributes?.SetTag("network.protocol.name", $"{response.Version.Major}.{response.Version.Minor}");

            return await RetryCAEResponseIfRequiredAsync(response, requestInfo, cancellationToken, claims, activityForAttributes).ConfigureAwait(false);
        }


        /// <summary>
        /// The key for the event raised by tracing when an authentication challenge is received
        /// </summary>
        public const string AuthenticateChallengedEventKey = "com.microsoft.kiota.authenticate_challenge_received";

        internal const string RetryCountAttributeName = "http.request.resend_count";

        private async Task<HttpResponseMessage> RetryCAEResponseIfRequiredAsync(HttpResponseMessage response, RequestInformation requestInfo, CancellationToken cancellationToken, string? claims, Activity? activityForAttributes)
        {
            using var span = activitySource?.StartActivity(nameof(RetryCAEResponseIfRequiredAsync));
            if(response.StatusCode == HttpStatusCode.Unauthorized &&
                string.IsNullOrEmpty(claims) && // avoid infinite loop, we only retry once
                (requestInfo.Content?.CanSeek ?? true))
            {
                var responseClaims = ContinuousAccessEvaluation.GetClaims(response);
                if(string.IsNullOrEmpty(responseClaims))
                {
                    return response;
                }
                span?.AddEvent(new ActivityEvent(AuthenticateChallengedEventKey));
                activityForAttributes?.SetTag(RetryCountAttributeName, 1);
                requestInfo.Content?.Seek(0, SeekOrigin.Begin);
                await DrainAsync(response, cancellationToken).ConfigureAwait(false);
                return await GetHttpResponseMessageAsync(requestInfo, cancellationToken, activityForAttributes, responseClaims).ConfigureAwait(false);
            }
            return response;
        }

        private void SetBaseUrlForRequestInformation(RequestInformation requestInfo)
        {
            IDictionaryExtensions.AddOrReplace(requestInfo.PathParameters, "baseurl", BaseUrl!);
        }
        /// <inheritdoc/>
        public async Task<T?> ConvertToNativeRequestAsync<T>(RequestInformation requestInfo, CancellationToken cancellationToken = default)
        {
            await authProvider.AuthenticateRequestAsync(requestInfo, null, cancellationToken).ConfigureAwait(false);
            if(GetRequestMessageFromRequestInformation(requestInfo, null) is T result)
                return result;
            else throw new InvalidOperationException($"Could not convert the request information to a {typeof(T).Name}");
        }

        private HttpRequestMessage GetRequestMessageFromRequestInformation(RequestInformation requestInfo, Activity? activityForAttributes)
        {
            using var span = activitySource?.StartActivity(nameof(GetRequestMessageFromRequestInformation));
            SetBaseUrlForRequestInformation(requestInfo);// this method can also be called from a different context so ensure the baseUrl is added.
            activityForAttributes?.SetTag("http.request.method", requestInfo.HttpMethod.ToString());
            var requestUri = requestInfo.URI;
            activityForAttributes?.SetTag("url.scheme", requestUri.Scheme);
            activityForAttributes?.SetTag("server.address", requestUri.Host);
            if(obsOptions.IncludeEUIIAttributes)
                activityForAttributes?.SetTag("url.full", requestUri.ToString());
            var message = new HttpRequestMessage
            {
                Method = new HttpMethod(requestInfo.HttpMethod.ToString().ToUpperInvariant()),
                RequestUri = requestUri,
#if NET5_0_OR_GREATER
                Version = client.DefaultRequestVersion,
                VersionPolicy = client.DefaultVersionPolicy
#else
                Version = httpVersion
#endif
            };

            if(requestInfo.RequestOptions != null)
#if NET5_0_OR_GREATER
            {
                foreach(var option in requestInfo.RequestOptions)
                    message.Options.Set(new HttpRequestOptionsKey<IRequestOption>(option.GetType().FullName!), option);
            }
            message.Options.Set(new HttpRequestOptionsKey<IRequestOption>(typeof(ObservabilityOptions).FullName!), obsOptions);
#else
            {
                foreach(var option in requestInfo.RequestOptions)
                    IDictionaryExtensions.TryAdd(message.Properties, option.GetType().FullName!, option);
            }
            IDictionaryExtensions.TryAdd(message.Properties!, typeof(ObservabilityOptions).FullName, obsOptions);
#endif

            if(requestInfo.Content != null && requestInfo.Content != Stream.Null)
                message.Content = new StreamContent(requestInfo.Content);
            if(requestInfo.Headers != null)
                foreach(var header in requestInfo.Headers)
                    if(!message.Headers.TryAddWithoutValidation(header.Key, header.Value) && message.Content != null)
                        message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);// Try to add the headers we couldn't add to the HttpRequestMessage before to the HttpContent

            if(message.Content != null)
            {
                if(message.Content.Headers.TryGetValues("Content-Length", out var contentLenValues))
                {
                    var contentLenEnumerator = contentLenValues.GetEnumerator();
                    if(contentLenEnumerator.MoveNext() && int.TryParse(contentLenEnumerator.Current, out var contentLenValueInt))
                        activityForAttributes?.SetTag("http.request.body.size", contentLenValueInt);
                }
                if(message.Content.Headers.TryGetValues("Content-Type", out var contentTypeValues))
                {
                    var contentTypeEnumerator = contentTypeValues.GetEnumerator();
                    if(contentTypeEnumerator.MoveNext())
                        activityForAttributes?.SetTag("http.request.header.content-type", contentTypeEnumerator.Current);
                }
            }
            return message;
        }

        /// <summary>
        /// Enable the backing store with the provided <see cref="IBackingStoreFactory"/>
        /// </summary>
        /// <param name="backingStoreFactory">The <see cref="IBackingStoreFactory"/> to use</param>
        public void EnableBackingStore(IBackingStoreFactory backingStoreFactory)
        {
            pNodeFactory = ApiClientBuilder.EnableBackingStoreForParseNodeFactory(pNodeFactory) ?? throw new InvalidOperationException("Could not enable backing store for the parse node factory");
            sWriterFactory = ApiClientBuilder.EnableBackingStoreForSerializationWriterFactory(sWriterFactory) ?? throw new InvalidOperationException("Could not enable backing store for the serializer writer factory");
            if(backingStoreFactory != null)
                BackingStoreFactorySingleton.Instance = backingStoreFactory;
        }

        /// <summary>
        /// Dispose/cleanup the client
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose/cleanup the client
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            if(createdClient)
            {
                client?.Dispose();
            }
        }
    }
}
