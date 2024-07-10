// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

/// <summary>
/// This handlers decodes special characters in the request query parameters that had to be encoded due to RFC 6570 restrictions names before executing the request.
/// </summary>
public class ParametersNameDecodingHandler : DelegatingHandler
{
    /// <summary>
    /// The options to use when decoding parameters names in URLs
    /// </summary>
    internal ParametersNameDecodingOption EncodingOptions
    {
        get; set;
    }
    /// <summary>
    /// Constructs a new <see cref="ParametersNameDecodingHandler"/>
    /// </summary>
    /// <param name="options">An OPTIONAL <see cref="ParametersNameDecodingOption"/> to configure <see cref="ParametersNameDecodingHandler"/></param>
    public ParametersNameDecodingHandler(ParametersNameDecodingOption? options = default)
    {
        EncodingOptions = options ?? new();
    }


    ///<inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var options = request.GetRequestOption<ParametersNameDecodingOption>() ?? EncodingOptions;
        Activity? activity;
        if(request.GetRequestOption<ObservabilityOptions>() is { } obsOptions)
        {
            var activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(obsOptions.TracerInstrumentationName);
            activity = activitySource.StartActivity($"{nameof(ParametersNameDecodingHandler)}_{nameof(SendAsync)}");
            activity?.SetTag("com.microsoft.kiota.handler.parameters_name_decoding.enable", true);
        }
        else
        {
            activity = null;
        }
        try
        {
            if(!request.RequestUri!.Query.Contains("%") ||
                !options.Enabled ||
                options.ParametersToDecode == null || options.ParametersToDecode.Count == 0)
            {
                return base.SendAsync(request, cancellationToken);
            }

            var originalUri = request.RequestUri;
            var query = DecodeUriEncodedString(originalUri.Query, options.ParametersToDecode.ToArray());
            var decodedUri = new UriBuilder(originalUri.Scheme, originalUri.Host, originalUri.Port, originalUri.AbsolutePath, query).Uri;
            request.RequestUri = decodedUri;
            return base.SendAsync(request, cancellationToken);
        }
        finally
        {
            activity?.Dispose();
        }
    }
    private static readonly char[] EntriesSeparator = ['&'];
    private static readonly char[] ParameterSeparator = ['='];

    internal static string? DecodeUriEncodedString(string? original, char[] charactersToDecode)
    {
        // for some reason static analysis is not picking up the fact that string.IsNullOrEmpty is already checking for null
        if(original is null || original.Length == 0 || charactersToDecode == null || charactersToDecode.Length == 0)
            return original;

        var symbolsToReplace = new List<(string, string)>();
        foreach(var character in charactersToDecode)
        {
            var symbol = ($"%{Convert.ToInt32(character):X}", character.ToString());
            if(original.Contains(symbol.Item1))
            {
                symbolsToReplace.Add(symbol);
            }
        }

        var encodedParameterValues = new List<string>();
        var parts = original.TrimStart('?').Split(EntriesSeparator, StringSplitOptions.RemoveEmptyEntries);
        foreach(var part in parts)
        {
            var parameter = part.Split(ParameterSeparator, StringSplitOptions.RemoveEmptyEntries)[0];
            if(parameter.Contains("%")) // only pull out params with `%` (encoded)
            {
                encodedParameterValues.Add(parameter);
            }
        }

        foreach(var parameter in encodedParameterValues)
        {
            var updatedParameterName = parameter;
            foreach(var symbolToReplace in symbolsToReplace)
            {
                if(parameter.Contains(symbolToReplace.Item1))
                {
                    updatedParameterName = updatedParameterName.Replace(symbolToReplace.Item1, symbolToReplace.Item2);
                }
            }
            original = original.Replace(parameter, updatedParameterName);
        }

        return original;
    }
}
