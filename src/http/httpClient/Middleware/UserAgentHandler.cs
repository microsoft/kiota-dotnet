using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware
{
    /// <summary>
    /// <see cref="UserAgentHandler"/> appends the current library version to the user agent header.
    /// </summary>
    public class UserAgentHandler : DelegatingHandler
    {
        private readonly UserAgentHandlerOption _userAgentOption;
        /// <summary>
        /// Creates a new instance of the <see cref="UserAgentHandler"/> class
        /// </summary>
        /// <param name="userAgentHandlerOption">The <see cref="UserAgentHandlerOption"/> instance to configure the user agent extension</param>
        public UserAgentHandler(UserAgentHandlerOption? userAgentHandlerOption = null)
        {
            _userAgentOption = userAgentHandlerOption ?? new UserAgentHandlerOption();
        }
        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request == null)
                throw new ArgumentNullException(nameof(request));

            Activity? activity;
            if(request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
            {
                var activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
                activity = activitySource?.StartActivity($"{nameof(UserAgentHandler)}_{nameof(SendAsync)}");
                activity?.SetTag("com.microsoft.kiota.handler.useragent.enable", true);
            }
            else
            {
                activity = null;
            }

            try
            {
                var userAgentHandlerOption = request.GetRequestOption<UserAgentHandlerOption>() ?? _userAgentOption;

                bool isProductNamePresent = false;
                foreach(var userAgent in request.Headers.UserAgent)
                {
                    if(userAgentHandlerOption.ProductName.Equals(userAgent.Product?.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        isProductNamePresent = true;
                        break;
                    }
                }

                if(userAgentHandlerOption.Enabled && !isProductNamePresent)
                {
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue(userAgentHandlerOption.ProductName, userAgentHandlerOption.ProductVersion));
                }
                return base.SendAsync(request, cancellationToken);
            }
            finally
            {
                activity?.Dispose();
            }
        }
    }
}
