using System;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Authentication;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests;

public class ApiKeyAuthenticationProviderTests {
    [Fact]
    public void DefensiveProgramming() {
        Assert.Throws<ArgumentNullException>(() => new ApiKeyAuthenticationProvider(null, "param", ApiKeyAuthenticationProvider.KeyLocation.Header));
        Assert.Throws<ArgumentNullException>(() => new ApiKeyAuthenticationProvider("key", null, ApiKeyAuthenticationProvider.KeyLocation.Header));

        var value = new ApiKeyAuthenticationProvider("key", "param", ApiKeyAuthenticationProvider.KeyLocation.Header);
        Assert.ThrowsAsync<ArgumentNullException>(async () => await value.AuthenticateRequestAsync(null));
    }
    [Fact]
    public async Task AddsInHeader() {
        var value = new ApiKeyAuthenticationProvider("key", "param", ApiKeyAuthenticationProvider.KeyLocation.Header);
        var request = new RequestInformation {
            UrlTemplate = "https://localhost{?param1}",
        };
        await value.AuthenticateRequestAsync(request);
        Assert.False(request.URI.ToString().EndsWith("param=key", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("param", request.Headers.Keys);
    }
    [Fact]
    public async Task AddsInQueryParameters() {
        var value = new ApiKeyAuthenticationProvider("key", "param", ApiKeyAuthenticationProvider.KeyLocation.QueryParameter);
        var request = new RequestInformation {
            UrlTemplate = "https://localhost{?param1}",
        };
        await value.AuthenticateRequestAsync(request);
        Assert.EndsWith("?param=key", request.URI.ToString());
        Assert.DoesNotContain("param", request.Headers.Keys);
    }
    [Fact]
    public async Task AddsInQueryParametersWithOtherParameters() {
        var value = new ApiKeyAuthenticationProvider("key", "param", ApiKeyAuthenticationProvider.KeyLocation.QueryParameter);
        var request = new RequestInformation {
            UrlTemplate = "https://localhost{?param1}",
        };
        request.QueryParameters.Add("param1", "value1");
        await value.AuthenticateRequestAsync(request);
        Assert.EndsWith("&param=key", request.URI.ToString());
        Assert.DoesNotContain("param", request.Headers.Keys);
    }
}