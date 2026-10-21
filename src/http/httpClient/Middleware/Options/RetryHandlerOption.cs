// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options
{
    /// <summary>
    /// The retry request option class
    /// </summary>
    public class RetryHandlerOption : IRequestOption
    {
        internal const int DefaultDelay = 3;
        internal const int DefaultMaxRetry = 3;
        internal const int MaxMaxRetry = 10;
        internal const int MaxDelay = 180;
        private int _maxRetry = DefaultMaxRetry;
        private int _delay = DefaultDelay;
        private TimeSpan _retriesTimeLimit = TimeSpan.Zero;

        /// <summary>
        /// The waiting time in seconds before retrying a request with a maximum value of 180 seconds. This defaults to 3 seconds.
        /// </summary>
        public int Delay
        {
            get
            {
                return _delay;
            }
            set
            {
                if(value < 0)
                {
                    throw new InvalidOperationException($"Minimum value for {nameof(Delay)} property exceeded ");
                }

                if(value > MaxDelay)
                {
                    throw new InvalidOperationException($"Maximum value for {nameof(MaxDelay)} property exceeded ");
                }

                _delay = value;
            }
        }

        /// <summary>
        /// The maximum number of retries for a request with a maximum value of 10. This defaults to 3.
        /// </summary>
        public int MaxRetry
        {
            get
            {
                return _maxRetry;
            }
            set
            {
                if(value < 0)
                {
                    throw new InvalidOperationException($"Minimum value for {nameof(MaxRetry)} property exceeded ");
                }

                if(value > MaxMaxRetry)
                {
                    throw new InvalidOperationException($"Maximum value for {nameof(MaxMaxRetry)} property exceeded ");
                }
                _maxRetry = value;
            }
        }

        /// <summary>
        /// The maximum time allowed for request retries.
        /// </summary>
        public TimeSpan RetriesTimeLimit
        {
            get
            {
                return _retriesTimeLimit;
            }
            set
            {
                if(value < TimeSpan.Zero)
                {
                    throw new InvalidOperationException($"Minimum value for {nameof(RetriesTimeLimit)} property exceeded ");
                }

                _retriesTimeLimit = value;
            }
        }

        /// <summary>
        /// A delegate that's called to determine whether a request should be retried or not.
        /// The delegate method should accept a delay time in seconds of, number of retry attempts and <see cref="HttpResponseMessage"/> as its parameters and return a <see cref="bool"/>.
        /// This defaults to a function that returns true for 503, 504, and 429 status codes and false otherwise.
        /// </summary>
        public Func<int, int, HttpResponseMessage, bool> ShouldRetry { get; set; } = (_, _, response) => response.StatusCode switch
        {
            // By default, retry on 503, 504, and 429 status codes
            HttpStatusCode.ServiceUnavailable => true,
            HttpStatusCode.GatewayTimeout => true,
            (HttpStatusCode)429 => true,
            _ => false
        };
    }
}
