// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Kiota.Abstractions.Authentication;

/// <summary>
/// This authentication provider authenticates requests using HTTP Basic authentication.
/// </summary>
public class BasicAuthenticationProvider : IAuthenticationProvider
{
    private readonly string BasicAuthHeaderValue;
    private const string AuthorizationHeaderKey = "Authorization";

    /// <summary>
    /// Instantiates a new <see cref="BasicAuthenticationProvider"/> using the provided parameters.
    /// </summary>
    /// <param name="username">The username to use for authentication.</param>
    /// <param name="password">The password to use for authentication.</param>
    public BasicAuthenticationProvider(string username, string password)
    {
        if( string.IsNullOrEmpty(username) )
            throw new ArgumentNullException(nameof(username));
        if( string.IsNullOrEmpty(password) )
            throw new ArgumentNullException(nameof(password));

        BasicAuthHeaderValue = $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"))}";
    }


    /// <inheritdoc />
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
    {
        if( request == null )
            throw new ArgumentNullException(nameof(request));

        if( !request.Headers.ContainsKey(AuthorizationHeaderKey) ) {
            request.Headers.Add(AuthorizationHeaderKey, BasicAuthHeaderValue);
        }

        return Task.CompletedTask;
    }
}
