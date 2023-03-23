// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Kiota.Abstractions;

/// <summary>
/// Base class for all request builders
/// </summary>
public abstract class BaseRequestBuilder
{
    /// <summary>Path parameters for the request</summary>
    protected Dictionary<string, object> PathParameters { get; set; }
    /// <summary>The request adapter to use to execute the requests.</summary>
    protected IRequestAdapter RequestAdapter { get; set; }
    /// <summary>Url template to use to build the URL for the current request builder</summary>
    protected string UrlTemplate { get; set; }
    /// <summary>
    /// Instantiates a new <see cref="BaseRequestBuilder"/> class
    /// </summary>
    /// <param name="pathParameters">Path parameters for the request</param>
    /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
    /// <param name="urlTemplate">Url template to use to build the URL for the current request builder</param>
    protected BaseRequestBuilder(IRequestAdapter requestAdapter, string urlTemplate, Dictionary<string, object> pathParameters)
    {
        _ = pathParameters ?? throw new ArgumentNullException(nameof(pathParameters));
        _ = requestAdapter ?? throw new ArgumentNullException(nameof(requestAdapter));
        _ = urlTemplate ?? throw new ArgumentNullException(nameof(urlTemplate)); // empty is fine
        PathParameters = new Dictionary<string, object>(pathParameters);
        RequestAdapter = requestAdapter;
        UrlTemplate = urlTemplate;
    }
    /// <summary>
    /// Instantiates a new <see cref="BaseRequestBuilder"/> class
    /// </summary>
    /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
    /// <param name="urlTemplate">Url template to use to build the URL for the current request builder</param>
    /// <param name="rawUrl">The raw URL to use for the current request builder</param>
    protected BaseRequestBuilder(IRequestAdapter requestAdapter, string urlTemplate, string rawUrl):this(requestAdapter, urlTemplate, new Dictionary<string, object>() {
        { RequestInformation.RawUrlKey, rawUrl }
    })
    {
        if (string.IsNullOrEmpty(rawUrl))
        {
            throw new ArgumentNullException(nameof(rawUrl));
        }
    }
}