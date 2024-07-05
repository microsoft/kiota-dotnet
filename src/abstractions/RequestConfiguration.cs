// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Kiota.Abstractions;
/// <summary>
/// Request configuration type for the request builders
/// </summary>
/// <typeparam name="T">The type of the query parameters class.</typeparam>
public class RequestConfiguration<T> where T : class, new()
{
    /// <summary>Request headers</summary>
    public RequestHeaders Headers { get; set; } = new();
    /// <summary>Request options</summary>
    public IList<IRequestOption> Options { get; set; } = new List<IRequestOption>();
    /// <summary>Query parameters</summary>
    public T QueryParameters { get; set; } = new();
}