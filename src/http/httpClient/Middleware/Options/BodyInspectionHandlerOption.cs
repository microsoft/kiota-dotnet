// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.IO;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

/// <summary>
/// The Body Inspection Option allows the developer to inspect the body of the request and response.
/// </summary>
public class BodyInspectionHandlerOption : IRequestOption
{
    /// <summary>
    /// Gets or sets a value indicating whether the request body should be inspected.
    /// Note tht this setting increases memory usae as the request body is copied to a new stream.
    /// </summary>
    public bool InspectRequestBody { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the response body should be inspected.
    /// Note tht this setting increases memory usae as the request body is copied to a new stream.
    /// </summary>
    public bool InspectResponseBody { get; set; }

    /// <summary>
    /// Gets the request body stream for the current request. This stream is available
    /// only if InspectRequestBody is set to true and the request contains a body.
    /// This stream is not disposed of by kiota, you need to take care of that.
    /// Note that this stream is a copy of the original request body stream, which has
    /// impact on memory usage. Use adequately.
    /// </summary>
    public Stream? RequestBody { get; internal set; }

    /// <summary>
    /// Gets the response body stream for the current request. This stream is available
    /// only if InspectResponseBody is set to true.
    /// This stream is not disposed of by kiota, you need to take care of that.
    /// Note that this stream is a copy of the original request body stream, which has
    /// impact on memory usage. Use adequately.
    /// </summary>
    public Stream? ResponseBody { get; internal set; }
}
