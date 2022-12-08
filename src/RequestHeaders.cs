using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Kiota.Abstractions;

/// <summary>Represents a collection of request headers.</summary>
public class RequestHeaders : IDictionary<string,IEnumerable<string>> {
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
    /// <inheritdoc/>
    public ICollection<string> Keys => _headers.Keys;
    /// <inheritdoc/>
    public ICollection<IEnumerable<string>> Values => _headers.Values.Cast<IEnumerable<string>>().ToList();
    /// <inheritdoc/>
    public int Count => _headers.Count;
    /// <inheritdoc/>
    public bool IsReadOnly => false;
    /// <inheritdoc/>
    public IEnumerable<string> this[string key] { get => TryGetValue(key, out var result) ? result : null; set => Add(key, value); }

    /// <summary>
    /// Removes the specified value from the header with the specified name.
    /// </summary>
    /// <param name="headerName">The name of the header to remove the value from.</param>
    /// <param name="headerValue">The value to remove from the header.</param>
    public bool Remove(string headerName, string headerValue) {
        if(string.IsNullOrEmpty(headerName))
            throw new ArgumentNullException(nameof(headerName));
        if(headerValue == null)
            throw new ArgumentNullException(nameof(headerValue));
        if(_headers.TryGetValue(headerName, out var values)) {
            var result = values.Remove(headerValue);
            if (!values.Any())
                _headers.Remove(headerName);
            return result;
        }
        return false;
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
    /// <inheritdoc/>
    public bool ContainsKey(string key) => !string.IsNullOrEmpty(key) && _headers.ContainsKey(key);
    /// <inheritdoc/>
    public void Add(string key, IEnumerable<string> value) => Add(key, value?.ToArray());
    /// <inheritdoc/>
    public bool Remove(string key) {
        if(string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        return _headers.Remove(key);
    }
    /// <inheritdoc/>
    public bool TryGetValue(string key, out IEnumerable<string> value) {
        if(string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        if(_headers.TryGetValue(key, out var values)) {
            value = values;
            return true;
        }
        value = Array.Empty<string>();
        return false;
    }
    /// <inheritdoc/>
    public void Add(KeyValuePair<string, IEnumerable<string>> item) => Add(item.Key, item.Value);
    /// <inheritdoc/>
    public bool Contains(KeyValuePair<string, IEnumerable<string>> item) => TryGetValue(item.Key, out var values) && item.Value.All(x => values.Contains(x)) && values.Count() == item.Value.Count();
    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<string, IEnumerable<string>>[] array, int arrayIndex) => throw new NotImplementedException();
    /// <inheritdoc/>
    public bool Remove(KeyValuePair<string, IEnumerable<string>> item) {
        var result = false;
        foreach (var value in item.Value)
            result |= Remove(item.Key, value);
        return result;
    }
    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator() => new RequestHeadersEnumerator(_headers.GetEnumerator());
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    private sealed class RequestHeadersEnumerator : IEnumerator<KeyValuePair<string, IEnumerable<string>>> {
        private readonly Dictionary<string, HashSet<string>>.Enumerator _enumerator;
        public RequestHeadersEnumerator(Dictionary<string, HashSet<string>>.Enumerator enumerator)
        {
            _enumerator = enumerator;
        }
        public KeyValuePair<string, IEnumerable<string>> Current => new(_enumerator.Current.Key, _enumerator.Current.Value);

        object IEnumerator.Current => Current;

        public void Dispose() {
            _enumerator.Dispose();
            GC.SuppressFinalize(this);
        }
        public bool MoveNext() => _enumerator.MoveNext();
        public void Reset() => (_enumerator as IEnumerator)?.Reset();
    }
}
