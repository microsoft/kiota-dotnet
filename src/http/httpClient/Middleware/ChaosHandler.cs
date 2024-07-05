// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation that is used for simulating server failures.
    /// </summary>
    public class ChaosHandler : DelegatingHandler, IDisposable
    {
        private readonly Random _random;
        private readonly ChaosHandlerOption _chaosHandlerOptions;
        private List<HttpResponseMessage> _knownFailures = new();
        private const string Json = "application/json";

        /// <summary>
        /// Create a ChaosHandler.
        /// </summary>
        /// <param name="chaosHandlerOptions">Optional parameter to change default behavior of handler.</param>
        public ChaosHandler(ChaosHandlerOption? chaosHandlerOptions = null)
        {
            _chaosHandlerOptions = chaosHandlerOptions ?? new ChaosHandlerOption();
            _random = new Random(DateTime.Now.Millisecond);
            LoadKnownFailures(_chaosHandlerOptions.KnownChaos);
        }
        /// <summary>
        /// The key used for the open telemetry event.
        /// </summary>
        public const string ChaosHandlerTriggeredEventKey = "com.microsoft.kiota.chaos_handler_triggered";
        /// <summary>
        /// Sends the request
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request == null)
                throw new ArgumentNullException(nameof(request));

            // Select global or per request options
            var chaosHandlerOptions = request.GetRequestOption<ChaosHandlerOption>() ?? _chaosHandlerOptions;
            Activity? activity;
            if(request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
            {
                var activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
                activity = activitySource?.StartActivity($"{nameof(ChaosHandler)}_{nameof(SendAsync)}");
                activity?.SetTag("com.microsoft.kiota.handler.chaos.enable", true);
            }
            else
            {
                activity = null;
            }
            try
            {

                // Planned Chaos or Random?
                if(chaosHandlerOptions.PlannedChaosFactory != null && chaosHandlerOptions.PlannedChaosFactory(request) is HttpResponseMessage plannedResponse)
                {
                    plannedResponse.RequestMessage = request;
                    activity?.AddEvent(new(ChaosHandlerTriggeredEventKey));
                    activity?.SetTag("com.microsoft.kiota.handler.chaos.planned", true);
                    return plannedResponse;
                }
                else if(_random.Next(100) < chaosHandlerOptions.ChaosPercentLevel)
                {
                    var chaosResponse = CreateChaosResponse(chaosHandlerOptions.KnownChaos ?? _knownFailures!);
                    chaosResponse.RequestMessage = request;
                    activity?.AddEvent(new(ChaosHandlerTriggeredEventKey));
                    activity?.SetTag("com.microsoft.kiota.handler.chaos.planned", false);
                    return chaosResponse;
                }

                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                activity?.Dispose();
            }
        }

        private HttpResponseMessage CreateChaosResponse(List<HttpResponseMessage> knownFailures)
        {
            var responseIndex = _random.Next(knownFailures.Count);
            return knownFailures[responseIndex];
        }

        private void LoadKnownFailures(List<HttpResponseMessage>? knownFailures)
        {
            if(knownFailures?.Count > 0)
            {
                _knownFailures = knownFailures;
            }
            else
            {
                _knownFailures = new List<HttpResponseMessage>
                {
                    Create429TooManyRequestsResponse(new TimeSpan(0, 0, 3)),
                    Create503Response(new TimeSpan(0, 0, 3)),
                    Create504GatewayTimeoutResponse(new TimeSpan(0, 0, 3))
                };
            }
        }

        /// <summary>
        /// Create a HTTP status 429 response message
        /// </summary>
        /// <param name="retry"><see cref="TimeSpan"/> for retry condition header value</param>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 429 response</returns>
        public static HttpResponseMessage Create429TooManyRequestsResponse(TimeSpan retry)
        {
            var contentString = JsonSerializer.Serialize(new MainError
            {
                error = new Error
                {
                    Code = "activityLimitReached",
                    Message = "Client application has been throttled and should not attempt to repeat the request until an amount of time has elapsed."
                }
#if NET5_0_OR_GREATER
            }, SourceGenerationContext.Default.MainError);
#else
            });
#endif
            var throttleResponse = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)429,
                Content = new StringContent(contentString, Encoding.UTF8, Json)
            };
            throttleResponse.Headers.RetryAfter = new RetryConditionHeaderValue(retry);
            return throttleResponse;
        }

        /// <summary>
        /// Create a HTTP status 503 response message
        /// </summary>
        /// <param name="retry"><see cref="TimeSpan"/> for retry condition header value</param>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 503 response</returns>
        public static HttpResponseMessage Create503Response(TimeSpan retry)
        {
            var contentString = JsonSerializer.Serialize(new MainError
            {
                error = new Error
                {
                    Code = "serviceNotAvailable",
                    Message = "The service is temporarily unavailable for maintenance or is overloaded. You may repeat the request after a delay, the length of which may be specified in a Retry-After header."
                }
#if NET5_0_OR_GREATER
            }, SourceGenerationContext.Default.MainError);
#else
            });
#endif
            var serverUnavailableResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Content = new StringContent(contentString, Encoding.UTF8, Json)
            };
            serverUnavailableResponse.Headers.RetryAfter = new RetryConditionHeaderValue(retry);
            return serverUnavailableResponse;
        }

        /// <summary>
        /// Create a HTTP status 502 response message
        /// </summary>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 502 Response</returns>
        public static HttpResponseMessage Create502BadGatewayResponse()
        {
            var contentString = JsonSerializer.Serialize(new MainError
            {
                error = new Error
                {
                    Code = "502"
                }
#if NET5_0_OR_GREATER
            }, SourceGenerationContext.Default.MainError);
#else
            });
#endif
            var badGatewayResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadGateway,
                Content = new StringContent(contentString, Encoding.UTF8, Json)
            };
            return badGatewayResponse;
        }

        /// <summary>
        /// Create a HTTP status 500 response message
        /// </summary>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 500 Response</returns>
        public static HttpResponseMessage Create500InternalServerErrorResponse()
        {
            var contentString = JsonSerializer.Serialize(new MainError
            {
                error = new Error
                {
                    Code = "generalException",
                    Message = "There was an internal server error while processing the request."
                }
#if NET5_0_OR_GREATER
            }, SourceGenerationContext.Default.MainError);
#else
            });
#endif
            var internalServerError = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(contentString, Encoding.UTF8, Json)
            };
            return internalServerError;
        }

        /// <summary>
        /// Create a HTTP status 504 response message
        /// </summary>
        /// <param name="retry"><see cref="TimeSpan"/> for retry condition header value</param>
        /// <returns>A <see cref="HttpResponseMessage"/> object simulating a 504 response</returns>
        public static HttpResponseMessage Create504GatewayTimeoutResponse(TimeSpan retry)
        {
            var contentString = JsonSerializer.Serialize(new MainError
            {
                error = new Error
                {
                    Code = "504",
                    Message = "The server, while acting as a proxy, did not receive a timely response from the upstream server it needed to access in attempting to complete the request. May occur together with 503."
                }
#if NET5_0_OR_GREATER
            }, SourceGenerationContext.Default.MainError);
#else
            });
#endif
            var gatewayTimeoutResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.GatewayTimeout,
                Content = new StringContent(contentString, Encoding.UTF8, Json)
            };
            gatewayTimeoutResponse.Headers.RetryAfter = new RetryConditionHeaderValue(retry);
            return gatewayTimeoutResponse;
        }

        /// <summary>
        /// Clean up any thing we created
        /// </summary>
        public new void Dispose()
        {
            // clean up the response messages
            foreach(var response in _knownFailures)
            {
                response.Dispose();
            }
            // Cleanup any base resources
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    internal partial class MainError
    {
        public Error error
        {
            get; set;
        } = new();
    }
    /// <summary>
    /// Private class to model sample responses
    /// </summary>
    internal partial class Error
    {
        /// <summary>
        /// The error code
        /// </summary>
        public string? Code
        {
            get; set;
        }

        /// <summary>
        /// The error message
        /// </summary>
        public string? Message
        {
            get; set;
        }
    }
#if NET5_0_OR_GREATER
        [JsonSerializable(typeof(MainError))]
        internal partial class SourceGenerationContext : JsonSerializerContext
        {
        }
#endif
}
