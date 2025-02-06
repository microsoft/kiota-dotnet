// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class RetryHandler : DelegatingHandler
    {
        private const string RetryAfter = "Retry-After";
        private const string RetryAttempt = "Retry-Attempt";

        /// <summary>
        /// RetryOption property
        /// </summary>
        internal RetryHandlerOption RetryOption
        {
            get; set;
        }

        /// <summary>
        /// Construct a new <see cref="RetryHandler"/>
        /// </summary>
        /// <param name="retryOption">An OPTIONAL <see cref="RetryHandlerOption"/> to configure <see cref="RetryHandler"/></param>
        public RetryHandler(RetryHandlerOption? retryOption = null)
        {
            RetryOption = retryOption ?? new RetryHandlerOption();
        }

        /// <summary>
        /// Send a HTTP request
        /// </summary>
        /// <param name="request">The HTTP request<see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <exception cref="AggregateException">Thrown when too many retries are performed.</exception>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request == null)
                throw new ArgumentNullException(nameof(request));

            var retryOption = request.GetRequestOption<RetryHandlerOption>() ?? RetryOption;
            ActivitySource? activitySource;
            Activity? activity;
            if(request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
            {
                activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
                activity = activitySource?.StartActivity($"{nameof(RetryHandler)}_{nameof(SendAsync)}");
                activity?.SetTag("com.microsoft.kiota.handler.retry.enable", true);
            }
            else
            {
                activity = null;
                activitySource = null;
            }

            try
            {
                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                // Check whether retries are permitted and that the MaxRetry value is a non - negative, non - zero value
                if(request.IsBuffered() && retryOption.MaxRetry > 0 && (ShouldRetry(response.StatusCode) || retryOption.ShouldRetry(retryOption.Delay, 0, response)))
                {
                    response = await SendRetryAsync(response, retryOption, cancellationToken, activitySource).ConfigureAwait(false);
                }

                return response;
            }
            finally
            {
                activity?.Dispose();
            }
        }

        /// <summary>
        /// Retry sending the HTTP request
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> which is returned and includes the HTTP request needs to be retried.</param>
        /// <param name="retryOption">The <see cref="RetryHandlerOption"/> for the retry.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the retry.</param>
        /// <param name="activitySource">The <see cref="ActivitySource"/> for the retry.</param>
        /// <exception cref="AggregateException">Thrown when too many retries are performed.</exception>"
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendRetryAsync(HttpResponseMessage response, RetryHandlerOption retryOption, CancellationToken cancellationToken, ActivitySource? activitySource)
        {
            int retryCount = 0;
            TimeSpan cumulativeDelay = TimeSpan.Zero;
            List<Exception> exceptions = new();

            while(retryCount < retryOption.MaxRetry)
            {
                exceptions.Add(await GetInnerExceptionAsync(response).ConfigureAwait(false));
                using var retryActivity = activitySource?.StartActivity($"{nameof(RetryHandler)}_{nameof(SendAsync)} - attempt {retryCount}");
                retryActivity?.SetTag(HttpClientRequestAdapter.RetryCountAttributeName, retryCount);
                retryActivity?.SetTag("http.response.status_code", response.StatusCode);

                // Call Delay method to get delay time from response's Retry-After header or by exponential backoff
                var delay = DelayAsync(response, retryCount, retryOption.Delay, out double delayInSeconds, cancellationToken);
                retryActivity?.SetTag("http.request.resend_delay", delayInSeconds);

                // If client specified a retries time limit, let's honor it
                if(retryOption.RetriesTimeLimit > TimeSpan.Zero)
                {
                    // Get the cumulative delay time
                    cumulativeDelay += TimeSpan.FromSeconds(delayInSeconds);

                    // Check whether delay will exceed the client-specified retries time limit value
                    if(cumulativeDelay > retryOption.RetriesTimeLimit)
                    {
                        return response;
                    }
                }

                // general clone request with internal CloneAsync (see CloneAsync for details) extension method
                var originalRequest = response.RequestMessage;
                if(originalRequest == null)
                {
                    return response;// We can't clone the original request to replay it.
                }
                var request = await originalRequest.CloneAsync(cancellationToken).ConfigureAwait(false);

                // Increase retryCount and then update Retry-Attempt in request header
                retryCount++;
                AddOrUpdateRetryAttempt(request, retryCount);

                // Delay time
                await delay.ConfigureAwait(false);

                // Call base.SendAsync to send the request
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if(!(request.IsBuffered() && (ShouldRetry(response.StatusCode) || retryOption.ShouldRetry(retryOption.Delay, retryCount, response))))
                {
                    return response;
                }
            }

            exceptions.Add(await GetInnerExceptionAsync(response).ConfigureAwait(false));

            throw new AggregateException($"Too many retries performed. More than {retryCount} retries encountered while sending the request.", exceptions);
        }

        /// <summary>
        /// Update Retry-Attempt header in the HTTP request
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <param name="retryCount">Retry times</param>
        private static void AddOrUpdateRetryAttempt(HttpRequestMessage request, int retryCount)
        {
            if(request.Headers.Contains(RetryAttempt))
            {
                request.Headers.Remove(RetryAttempt);
            }
            request.Headers.Add(RetryAttempt, retryCount.ToString());
        }

        /// <summary>
        /// Delay task operation for timed-retries based on Retry-After header in the response or exponential back-off
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>returned.</param>
        /// <param name="retryCount">The retry counts</param>
        /// <param name="delay">Delay value in seconds.</param>
        /// <param name="delayInSeconds"></param>
        /// <param name="cancellationToken">The cancellationToken for the Http request</param>
        /// <returns>The <see cref="Task"/> for delay operation.</returns>
        static internal Task DelayAsync(HttpResponseMessage response, int retryCount, int delay, out double delayInSeconds, CancellationToken cancellationToken)
        {
            delayInSeconds = delay;
            if(response.Headers.TryGetValues(RetryAfter, out var values))
            {
                using var v = values.GetEnumerator();
                string retryAfter = v.MoveNext() ? v.Current : throw new InvalidOperationException("Retry-After header is empty.");
                // the delay could be in the form of a seconds or a http date. See https://httpwg.org/specs/rfc7231.html#header.retry-after
                if(int.TryParse(retryAfter, out int delaySeconds))
                {
                    delayInSeconds = delaySeconds;
                }
                else if(DateTime.TryParseExact(retryAfter, CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                {
                    var timeSpan = dateTime - DateTime.Now;
                    // ensure the delay is a positive span otherwise use the exponential back-off
                    delayInSeconds = timeSpan.Seconds > 0 ? timeSpan.Seconds : CalculateExponentialDelay(retryCount, delay);
                }
            }
            else
            {
                delayInSeconds = CalculateExponentialDelay(retryCount, delay);
            }

            var delayTimeSpan = TimeSpan.FromSeconds(Math.Min(delayInSeconds, RetryHandlerOption.MaxDelay));
            delayInSeconds = delayTimeSpan.TotalSeconds;
            return Task.Delay(delayTimeSpan, cancellationToken);
        }

        /// <summary>
        /// Calculates the delay based on the exponential back off
        /// </summary>
        /// <param name="retryCount">The retry count</param>
        /// <param name="delay">The base to use as a delay</param>
        /// <returns></returns>
        private static double CalculateExponentialDelay(int retryCount, int delay)
        {
            return Math.Pow(2, retryCount) * delay;
        }

        /// <summary>
        /// Check the HTTP status to determine whether it should be retried or not.
        /// </summary>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>returned.</param>
        /// <returns></returns>
        private static bool ShouldRetry(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.ServiceUnavailable => true,
                HttpStatusCode.GatewayTimeout => true,
                (HttpStatusCode)429 => true,
                _ => false
            };
        }

        private static async Task<Exception> GetInnerExceptionAsync(HttpResponseMessage response)
        {
            string? errorMessage = null;

            // Drain response content to free connections. Need to perform this
            // before retry attempt and before the TooManyRetries ServiceException.
            if(response.Content != null)
            {
                errorMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var headersDictionary = new Dictionary<string, IEnumerable<string>>();
            foreach(var header in response.Headers)
            {
                headersDictionary.Add(header.Key, header.Value);
            }

            return new ApiException($"HTTP request failed with status code: {response.StatusCode}.{errorMessage}")
            {
                ResponseStatusCode = (int)response.StatusCode,
                ResponseHeaders = headersDictionary,
            };
        }
    }
}
