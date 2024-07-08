// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Http.HttpClientLibrary;

/// <summary>
/// Holds the tracing, metrics and logging configuration for the request adapter
/// </summary>
public class ObservabilityOptions : IRequestOption {
    /// <summary>
    /// Gets or sets a value indicating whether to include attributes which could contain EUII information.
    /// </summary>
    public bool IncludeEUIIAttributes { get; set; }
    private static readonly Lazy<string> _name = new Lazy<string>(() => typeof(ObservabilityOptions).Namespace!);
    /// <summary>
    /// Gets the observability name to use for the tracer
    /// </summary>
    public string TracerInstrumentationName => _name.Value;
}
