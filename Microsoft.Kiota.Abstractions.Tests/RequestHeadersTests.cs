using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests;

public class RequestHeadersTests {
    [Fact]
    public void Defensive() {
        var instance = new RequestHeaders();
        Assert.Throws<ArgumentNullException>(() => instance.Add(null, "value"));
        Assert.Throws<ArgumentNullException>(() => instance.Add("name", (string[])null));
        instance.Add("name", new string[]{});
        instance.Add("name", new List<string>());
        instance.Add(new KeyValuePair<string, IEnumerable<string>>("name", new string[]{}));
        Assert.Throws<ArgumentNullException>(() => instance[null]);
        Assert.Throws<ArgumentNullException>(() => instance.Remove(null));
        Assert.Throws<ArgumentNullException>(() => instance.Remove(null, "value"));
        Assert.Throws<ArgumentNullException>(() => instance.Remove("name", null));
        Assert.Throws<ArgumentNullException>(() => instance.Update(null));
    }
    [Fact]
    public void AddsToNonExistent() {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        Assert.Equal(new [] { "value" }, instance["name"]);
    }
    [Fact]
    public void AddsToExistent() {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        Assert.Equal(new [] { "value", "value2" }, instance["name"]);
    }
    [Fact]
    public void RemovesValue() {
        var instance = new RequestHeaders();
        instance.Remove("name", "value");
        instance.Add("name", "value");
        instance.Add("name", "value2");
        instance.Remove("name", "value");
        Assert.Equal(new [] { "value2" }, instance["name"]);
        instance.Remove("name", "value2");
        Assert.Null(instance["name"]);
    }
    [Fact]
    public void Removes() {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        Assert.True(instance.Remove("name"));
        Assert.Null(instance["name"]);
        Assert.False(instance.Remove("name"));
    }
    [Fact]
    public void Clears() {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        instance.Clear();
        Assert.Null(instance["name"]);
    }
    [Fact]
    public void GetsEnumerator() {
        var instance = new RequestHeaders();
        instance.Add("name", "value");
        instance.Add("name", "value2");
        using var enumerator = instance.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Equal("name", enumerator.Current.Key);
        Assert.Equal(new [] { "value", "value2" }, enumerator.Current.Value);
        Assert.False(enumerator.MoveNext());
    }
}