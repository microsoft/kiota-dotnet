using System;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests;

public class RequestHeadersTests {
    [Fact]
    public void Defensive() {
        var instance = new RequestHeaders();
        Assert.Throws<ArgumentNullException>(() => instance.Add(null, "value"));
        Assert.Throws<ArgumentNullException>(() => instance.Add("name", (string[])null));
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
}