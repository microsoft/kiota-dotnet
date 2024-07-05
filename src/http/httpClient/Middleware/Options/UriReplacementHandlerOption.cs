using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

/// <summary>
/// Interface for making URI replacements.
/// </summary>
public interface IUriReplacementHandlerOption : IRequestOption
{
    /// <summary>
    /// Check if URI replacement is enabled for the option.
    /// </summary>
    /// <returns>true if replacement is enabled or false otherwise.</returns>
    bool IsEnabled();

    /// <summary>
    /// Accepts a URI and returns a new URI with all replacements applied.
    /// </summary>
    /// <param name="original">The URI to apply replacements to</param>
    /// <returns>A new URI with all replacements applied.</returns>
    Uri? Replace(Uri? original);
}

/// <summary>
/// Url replacement options.
/// </summary>
public class UriReplacementHandlerOption : IUriReplacementHandlerOption
{
    private readonly bool isEnabled;

    private readonly IEnumerable<KeyValuePair<string, string>> replacementPairs;

    /// <summary>
    /// Creates a new instance of UriReplacementOption.
    /// </summary>
    /// <param name="isEnabled">Whether replacement is enabled.</param>
    /// <param name="replacementPairs">Replacements with the key being a string to match against and the value being the replacement.</param>
    public UriReplacementHandlerOption(bool isEnabled, IEnumerable<KeyValuePair<string, string>> replacementPairs)
    {
        this.isEnabled = isEnabled;
        this.replacementPairs = replacementPairs;
    }

    /// <summary>
    /// Creates a new instance of UriReplacementOption with no replacements.
    /// </summary>
    /// <param name="isEnabled">Whether replacement is enabled.</param>
    /// <remarks>Replacement is disabled by default.</remarks>
    public UriReplacementHandlerOption(bool isEnabled = false) : this(isEnabled, Array.Empty<KeyValuePair<string, string>>()) { }

    /// <inheritdoc/>
    public bool IsEnabled()
    {
        return isEnabled;
    }

    /// <inheritdoc/>
    public Uri? Replace(Uri? original)
    {
        if(original is null) return null;

        if(!isEnabled)
        {
            return original;
        }

        var newUrl = new UriBuilder(original);
        foreach(var pair in replacementPairs)
        {
            newUrl.Path = newUrl.Path.Replace(pair.Key, pair.Value);
        }

        return newUrl.Uri;
    }
}
