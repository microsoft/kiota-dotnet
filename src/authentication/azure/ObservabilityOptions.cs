// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;

namespace Microsoft.Kiota.Authentication.Azure;
/// <summary>
/// Holds the tracing, metrics and logging configuration for the authentication provider
/// </summary>
public class ObservabilityOptions
{
    private static readonly Lazy<string> _name = new Lazy<string>(() => typeof(ObservabilityOptions).Namespace!);
    /// <summary>
    /// Gets the observability name to use for the tracer
    /// </summary>
    public string TracerInstrumentationName => _name.Value;
}
