// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Microsoft.Kiota.Authentication.Azure;
/// <summary>
/// Provides an implementation of <see cref="IAccessTokenProvider"/> for Azure.Identity.
/// </summary>
public class AzureIdentityAccessTokenProvider : IAccessTokenProvider, IDisposable
{
    private static readonly object BoxedTrue = true;
    private static readonly object BoxedFalse = false;

    private readonly TokenCredential _credential;
    private readonly ActivitySource _activitySource;
    private readonly bool _isCaeEnabled;
    private readonly HashSet<string> _scopes;
    private readonly Func<Uri, Dictionary<string, object>?, TokenRequestContext, TokenRequestContext>? _tokenRequestContextFactory;
    /// <inheritdoc />
    public AllowedHostsValidator AllowedHostsValidator { get; protected set; }

    /// <summary>
    /// The <see cref="AzureIdentityAccessTokenProvider"/> constructor
    /// </summary>
    /// <param name="credential">The credential implementation to use to obtain the access token.</param>
    /// <param name="options">The options to use for the access token provider.</param>
    /// <param name="scopes">The scopes to request the access token for.</param>
    public AzureIdentityAccessTokenProvider(TokenCredential credential, AzureIdentityAccessTokenProviderOptions? options, params string[] scopes)
    {
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));

        AllowedHostsValidator = new AllowedHostsValidator(options?.AllowedHosts);

        if(scopes == null)
            _scopes = new();
        else
            _scopes = new(scopes, StringComparer.OrdinalIgnoreCase);

        _activitySource = new((options?.ObservabilityOptions ?? new()).TracerInstrumentationName);
        _isCaeEnabled = options?.IsCaeEnabled ?? true;
        _tokenRequestContextFactory = options?.TokenRequestContextFactory;
    }
    /// <summary>
    /// The <see cref="AzureIdentityAccessTokenProvider"/> constructor
    /// </summary>
    /// <param name="credential">The credential implementation to use to obtain the access token.</param>
    /// <param name="allowedHosts">The list of allowed hosts for which to request access tokens.</param>
    /// <param name="scopes">The scopes to request the access token for.</param>
    /// <param name="observabilityOptions">The observability options to use for the authentication provider.</param>
    /// <param name="isCaeEnabled">Whether to enable Conditional Access Evaluation (CAE) for the token request.</param>
    [Obsolete("This constructor is obsolete and will be removed in a future version. Use the constructor that takes AzureIdentityAccessTokenProviderOptions instead.")]
    public AzureIdentityAccessTokenProvider(TokenCredential credential, string[]? allowedHosts = null, ObservabilityOptions? observabilityOptions = null, bool isCaeEnabled = true, params string[] scopes)
        : this(credential, new AzureIdentityAccessTokenProviderOptions { AllowedHosts = allowedHosts, ObservabilityOptions = observabilityOptions, IsCaeEnabled = isCaeEnabled }, scopes)
    {
    }
    /// <summary>
    /// The <see cref="AzureIdentityAccessTokenProvider"/> constructor
    /// </summary>
    /// <param name="credential">The credential implementation to use to obtain the access token.</param>
    /// <param name="allowedHosts">The list of allowed hosts for which to request access tokens.</param>
    /// <param name="scopes">The scopes to request the access token for.</param>
    /// <param name="observabilityOptions">The observability options to use for the authentication provider.</param>
    [Obsolete("This constructor is obsolete and will be removed in a future version. Use the constructor that takes AzureIdentityAccessTokenProviderOptions instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public AzureIdentityAccessTokenProvider(TokenCredential credential, string[]? allowedHosts, ObservabilityOptions? observabilityOptions, params string[] scopes) :
    this(credential, allowedHosts, observabilityOptions, true, scopes)
    {

    }

    private const string ClaimsKey = "claims";

    private readonly HashSet<string> _localHostStrings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "[::1]",
        "::1",
        "127.0.0.1"
    };

    /// <inheritdoc/>
    public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
    {
        using var span = _activitySource?.StartActivity(nameof(GetAuthorizationTokenAsync));
        if(!AllowedHostsValidator.IsUrlHostValid(uri))
        {
            span?.SetTag("com.microsoft.kiota.authentication.is_url_valid", BoxedFalse);
            return string.Empty;
        }

        if(!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) && !_localHostStrings.Contains(uri.Host))
        {
            span?.SetTag("com.microsoft.kiota.authentication.is_url_valid", BoxedFalse);
            throw new ArgumentException("Only https is supported");
        }
        span?.SetTag("com.microsoft.kiota.authentication.is_url_valid", BoxedTrue);

        string? decodedClaim = null;
        if(additionalAuthenticationContext is not null &&
                    additionalAuthenticationContext.ContainsKey(ClaimsKey) &&
                    additionalAuthenticationContext[ClaimsKey] is string claims)
        {
            span?.SetTag("com.microsoft.kiota.authentication.additional_claims_provided", BoxedTrue);
            var decodedBase64Bytes = Convert.FromBase64String(claims);
            decodedClaim = Encoding.UTF8.GetString(decodedBase64Bytes);
        }
        else
            span?.SetTag("com.microsoft.kiota.authentication.additional_claims_provided", BoxedFalse);

        string[] scopes;
        if(_scopes.Count > 0)
        {
            scopes = new string[_scopes.Count];
            _scopes.CopyTo(scopes);
        }
        else
            scopes = [$"{uri.Scheme}://{uri.Host}/.default"];

        var tokenRequestContext = new TokenRequestContext(scopes, claims: decodedClaim, isCaeEnabled: _isCaeEnabled);
        if(_tokenRequestContextFactory != null)
            tokenRequestContext = _tokenRequestContextFactory(uri, additionalAuthenticationContext, tokenRequestContext);

        span?.SetTag("com.microsoft.kiota.authentication.scopes", string.Join(",", tokenRequestContext.Scopes));

        var result = await _credential.GetTokenAsync(tokenRequestContext, cancellationToken).ConfigureAwait(false);
        return result.Token;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _activitySource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
