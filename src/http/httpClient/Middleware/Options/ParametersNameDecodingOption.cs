// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
/// <summary>
/// The ParametersEncodingOption request class
/// </summary>
public class ParametersNameDecodingOption : IRequestOption
{
    /// <summary>
    /// Whether to decode the specified characters in the request query parameters names
    /// </summary>
    public bool Enabled { get; set; } = true;
    /// <summary>
    /// The list of characters to decode in the request query parameters names before executing the request
    /// </summary>
    public List<char> ParametersToDecode { get; set; } = new() { '$' }; // '.', '-', '~' already being decoded by Uri
}
