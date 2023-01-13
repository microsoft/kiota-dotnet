// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Abstractions.Authentication;
/// <summary>
///     Provides a base class for implementing <see cref="IAuthenticationProvider" /> for Bearer token scheme.
/// </summary>
public class BaseBearerTokenAuthenticationProvider : IAuthenticationProvider
{
    /// <summary>
    /// Creates a new instance of <see cref="BaseBearerTokenAuthenticationProvider"/>.
    /// </summary>
    /// <param name="accessTokenProvider">The <see cref="IAccessTokenProvider"/> to use for getting the access token.</param>
    public BaseBearerTokenAuthenticationProvider(IAccessTokenProvider accessTokenProvider)
    {
        AccessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
    }
    /// <summary>
    ///     Gets the <see cref="IAccessTokenProvider" /> to use for getting the access token.
    /// </summary>
    public IAccessTokenProvider AccessTokenProvider {get; private set;}
    private const string AuthorizationHeaderKey = "Authorization";
    private const string ClaimsKey = "claims";

    /// <inheritdoc />
    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
    {
        if(request == null) throw new ArgumentNullException(nameof(request));
        if(additionalAuthenticationContext != null &&
            additionalAuthenticationContext.ContainsKey(ClaimsKey) &&
            request.Headers.ContainsKey(AuthorizationHeaderKey))
            request.Headers.Remove(AuthorizationHeaderKey);

        if(!request.Headers.ContainsKey(AuthorizationHeaderKey))
        {
            var token = await AccessTokenProvider.GetAuthorizationTokenAsync(request.URI, additionalAuthenticationContext, cancellationToken);
            if(!string.IsNullOrEmpty(token))
                request.Headers.Add(AuthorizationHeaderKey, $"Bearer {token}");
        }
    }
}
