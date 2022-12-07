using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Kiota.Abstractions;

/// <summary>Represents a collection of request headers.</summary>
public class RequestHeaders {
    internal Dictionary<string, HashSet<string>> _headers = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Adds values to the header with the specified name.
    /// </summary>
    /// <param name="headerName">The name of the header to add values to.</param>
    /// <param name="headerValues">The values to add to the header.</param>
    public void Add(string headerName, params string[] headerValues) {
        if(string.IsNullOrEmpty(headerName))
            throw new ArgumentNullException(nameof(headerName));
        if(headerValues == null)
            throw new ArgumentNullException(nameof(headerValues));
        if(!headerValues.Any())
            return;
        if(_headers.TryGetValue(headerName, out var values))
            foreach(var headerValue in headerValues)
                values.Add(headerValue);
        else
            _headers.Add(headerName, new HashSet<string>(headerValues));
    }
    /// <summary>
    /// Gets the names of the headers.
    /// </summary>
    public IEnumerable<string> Keys => _headers.Keys;
    /// <summary>
    /// Gets values of the header with the specified name.
    /// </summary>
    /// <param name="headerName">The name of the header to get values from.</param>
    /// <returns>The values of the header with the specified name.</returns>
    public IEnumerable<string> Get(string headerName) {
        if(string.IsNullOrEmpty(headerName))
            throw new ArgumentNullException(nameof(headerName));
        if(_headers.TryGetValue(headerName, out var values))
            return values;
        return Array.Empty<string>();
    }
    /// <summary>
    /// Removes all values for the header with the specified name.
    /// </summary>
    /// <param name="headerName">The name of the header to remove.</param>
    public void Remove(string headerName) {
        if(string.IsNullOrEmpty(headerName))
            throw new ArgumentNullException(nameof(headerName));
        _headers.Remove(headerName);
    }
    /// <summary>
    /// Removes the specified value from the header with the specified name.
    /// </summary>
    /// <param name="headerName">The name of the header to remove the value from.</param>
    /// <param name="headerValue">The value to remove from the header.</param>
    public void RemoveValue(string headerName, string headerValue) {
        if(string.IsNullOrEmpty(headerName))
            throw new ArgumentNullException(nameof(headerName));
        if(headerValue == null)
            throw new ArgumentNullException(nameof(headerValue));
        if(_headers.TryGetValue(headerName, out var values)) {
            values.Remove(headerValue);
            if (!values.Any())
                _headers.Remove(headerName);
        }
    }
    /// <summary>
    /// Updates the headers with the values from the specified headers.
    /// </summary>
    /// <param name="headers">The headers to update the current headers with.</param>
    public void Update(RequestHeaders headers) {
        _headers = headers?._headers ?? throw new ArgumentNullException(nameof(headers));
    }
    /// <summary>
    /// Removes all headers.
    /// </summary>
    public void Clear() {
        _headers.Clear();
    }
    /// <summary>
    /// Checks if the headers contain a header with the specified name.
    /// </summary>
    /// <param name="key">The name of the header to check for.</param>
    /// <returns>true if the headers contain a header with the specified name, false otherwise.</returns>
    public bool ContainsKey(string key) => string.IsNullOrEmpty(key) ? false : _headers.ContainsKey(key);
}
