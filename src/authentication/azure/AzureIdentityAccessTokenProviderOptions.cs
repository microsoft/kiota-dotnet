// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Azure.Core;

namespace Microsoft.Kiota.Authentication.Azure;
/// <summary>
/// Options for configuring an <see cref="AzureIdentityAccessTokenProvider"/> instance.
/// </summary>
public class AzureIdentityAccessTokenProviderOptions
{
    /// <summary>
    /// The list of allowed hosts for which to request access tokens.
    /// </summary>
    public string[]? AllowedHosts { get; set; }

    /// <summary>
    /// The observability options to use for the authentication provider.
    /// </summary>
    public ObservabilityOptions? ObservabilityOptions { get; set; }

    /// <summary>
    /// Whether to enable Conditional Access Evaluation (CAE) for the token request. Defaults to true.
    /// </summary>
    public bool IsCaeEnabled { get; set; } = true;

    /// <summary>
    /// A callback to customize the <see cref="TokenRequestContext"/> before requesting a token.
    /// </summary>
    public Func<Uri, Dictionary<string, object>?, TokenRequestContext, TokenRequestContext>? TokenRequestContextFactory { get; set; }
}
