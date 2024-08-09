// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using Azure.Core;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Microsoft.Kiota.Authentication.Azure;
/// <summary>
/// The <see cref="BaseBearerTokenAuthenticationProvider"/> implementation that supports implementations of <see cref="TokenCredential"/> from Azure.Identity.
/// </summary>
public class AzureIdentityAuthenticationProvider : BaseBearerTokenAuthenticationProvider
{
    /// <summary>
    /// The <see cref="AzureIdentityAuthenticationProvider"/> constructor
    /// </summary>
    /// <param name="credential">The credential implementation to use to obtain the access token.</param>
    /// <param name="allowedHosts">The list of allowed hosts for which to request access tokens.</param>
    /// <param name="scopes">The scopes to request the access token for.</param>
    /// <param name="isCaeEnabled">Whether to enable Conditional Access Evaluation (CAE) for the token request.</param>
    /// <param name="observabilityOptions">The observability options to use for the authentication provider.</param>
    public AzureIdentityAuthenticationProvider(TokenCredential credential, string[]? allowedHosts = null, ObservabilityOptions? observabilityOptions = null, bool isCaeEnabled = true, params string[] scopes)
        : base(new AzureIdentityAccessTokenProvider(credential, allowedHosts, observabilityOptions, isCaeEnabled, scopes))
    {
    }
    /// <summary>
    /// The <see cref="AzureIdentityAuthenticationProvider"/> constructor
    /// </summary>
    /// <param name="credential">The credential implementation to use to obtain the access token.</param>
    /// <param name="allowedHosts">The list of allowed hosts for which to request access tokens.</param>
    /// <param name="scopes">The scopes to request the access token for.</param>
    /// <param name="observabilityOptions">The observability options to use for the authentication provider.</param>
    [Obsolete("This constructor is obsolete and will be removed in a future version. Use the constructor that takes an isCaeEnabled parameter instead.")]
    public AzureIdentityAuthenticationProvider(TokenCredential credential, string[]? allowedHosts, ObservabilityOptions? observabilityOptions, params string[] scopes)
        : this(credential, allowedHosts, observabilityOptions, true, scopes)
    {
    }
}
