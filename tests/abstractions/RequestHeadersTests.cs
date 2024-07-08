using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests;

public class RequestHeadersTests
{
    [Fact]
    public void Defensive()
    {
        var instance = new RequestHeaders();
        Assert.Throws<ArgumentNullException>(() => instance.Add(null, "value"));
        Assert.Throws<ArgumentNullException>(() => instance.Add("name", (string[])null));
        instance.Add("name", Array.Empty<string>());
        instance.Add("name", new List<string>());
        instance.Add(new KeyValuePair<string, IEnumerable<string>>("name", Array.Empty<string>()));
        Assert.Throws<ArgumentNullException>(() => instance[null]);
        Assert.Throws<ArgumentNullException>(() => instance.Remove(null));
        Assert.Throws<ArgumentNullException>(() => instance.Remove(null, "value"));
        Assert.Throws<ArgumentNullException>(() => instance.Remove("name", null));
        Assert.Throws<ArgumentNullException>(() => instance.AddAll(null));
        instance.ContainsKey(null);
    }
    [Fact]
    public void AddsToNonExistent()
    {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        Assert.Equal(new[] { "value" }, instance["name"]);
    }
    [Fact]
    public void TryAddsToNonExistent()
    {
        var instance = new RequestHeaders();
        var result = instance.TryAdd("name", "value");
        Assert.True(result);
        Assert.Equal(new[] { "value" }, instance["name"]);
    }
    [Fact]
    public void AddsToExistent()
    {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        Assert.Equal(new[] { "value", "value2" }, instance["name"]);
    }
    [Fact]
    public void TryAddsToExistent()
    {
        var instance = new RequestHeaders();
        var result = instance.TryAdd("name", "value");
        Assert.True(result);
        result = instance.TryAdd("name", "value2");
        Assert.False(result);
        Assert.Equal(new[] { "value" }, instance["name"]);
    }
    [Fact]
    public void AddsSingleValueHeaderToExistent()
    {
        var instance = new RequestHeaders();
        instance.Add("Content-Type", "value");
        instance.Add("Content-Type", "value2");
        Assert.Equal(new[] { "value2" }, instance["Content-Type"]);
    }
    [Fact]
    public void TryAddsSingleValueHeaderToExistent()
    {
        var instance = new RequestHeaders();
        instance.TryAdd("Content-Type", "value");
        instance.TryAdd("Content-Type", "value2");
        Assert.Equal(new[] { "value" }, instance["Content-Type"]);
    }
    [Fact]
    public void RemovesValue()
    {
        var instance = new RequestHeaders();
        instance.Remove("name", "value");
        instance.Add("name", "value");
        instance.Add("name", "value2");
        instance.Remove("name", "value");
        Assert.Equal(new[] { "value2" }, instance["name"]);
        instance.Remove("name", "value2");
        Assert.Throws<KeyNotFoundException>(() => instance["name"]);
    }
    [Fact]
    public void Removes()
    {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        Assert.True(instance.Remove("name"));
        Assert.Throws<KeyNotFoundException>(() => instance["name"]);
        Assert.False(instance.Remove("name"));
    }
    [Fact]
    public void RemovesKVP()
    {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        Assert.True(instance.Remove(new KeyValuePair<string, IEnumerable<string>>("name", new[] { "value", "value2" })));
        Assert.Throws<KeyNotFoundException>(() => instance["name"]);
        Assert.False(instance.Remove("name"));
    }
    [Fact]
    public void Clears()
    {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        instance.Clear();
        Assert.Throws<KeyNotFoundException>(() => instance["name"]);
        Assert.Empty(instance.Keys);
    }
    [Fact]
    public void GetsEnumerator()
    {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        using var enumerator = instance.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Equal("name", enumerator.Current.Key);
        Assert.Equal(new[] { "value", "value2" }, enumerator.Current.Value);
        Assert.False(enumerator.MoveNext());
    }
    [Fact]
    public void Updates()
    {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        var instance2 = new RequestHeaders();
        instance2.AddAll(instance);
        Assert.NotEmpty(instance["name"]);
    }
}