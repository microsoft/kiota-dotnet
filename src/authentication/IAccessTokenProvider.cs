// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Abstractions.Authentication;

/// <summary>
/// Defines a contract for obtaining access tokens for a given url.
/// </summary>
public interface IAccessTokenProvider
{
    /// <summary>
    ///     This method is called by the <see cref="BaseBearerTokenAuthenticationProvider" /> class to get the access token.
    /// </summary>
    /// <param name="uri">The target URI to get an access token for.</param>
    /// <param name="additionalAuthenticationContext">Additional authentication context to pass to the authentication library.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    /// <returns>A Task that holds the access token to use for the request.</returns>
    Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = default, CancellationToken cancellationToken = default);
    /// <summary>
    /// Returns the <see cref="AllowedHostsValidator"/> for the provider.
    /// </summary>
    AllowedHostsValidator AllowedHostsValidator { get; }
}
