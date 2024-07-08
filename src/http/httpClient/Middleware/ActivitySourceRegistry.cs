// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

/// <summary>
/// Internal static registry for activity sources during tracing.
/// </summary>
internal class ActivitySourceRegistry
{
    private readonly ConcurrentDictionary<string, ActivitySource> _activitySources = new (StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The default instance of the registry
    /// </summary>
    public static readonly ActivitySourceRegistry DefaultInstance = new();

    /// <summary>
    /// Get an a <see cref="ActivitySource"/> instance with the given name or create one.
    /// </summary>
    /// <param name="sourceName">The name of the <see cref="ActivitySource"/></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">When the parameter is null or empty</exception>
    public ActivitySource GetOrCreateActivitySource(string sourceName)
    {
        if(string.IsNullOrEmpty(sourceName))
            throw new ArgumentNullException(nameof(sourceName));

        return _activitySources.GetOrAdd(sourceName, static source => new ActivitySource(source));

    }
}
