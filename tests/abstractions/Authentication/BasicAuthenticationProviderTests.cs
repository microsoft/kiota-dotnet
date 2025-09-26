using Microsoft.Kiota.Abstractions.Authentication;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests;

public class BasicAuthenticationProviderTests
{
    [Fact]
    public async Task DefensiveProgramming()
    {
        Assert.Throws<ArgumentNullException>(() => new BasicAuthenticationProvider(null!, "password"));
        Assert.Throws<ArgumentNullException>(() => new BasicAuthenticationProvider("username", null!));

        var value = new BasicAuthenticationProvider("username", "password");
        await Assert.ThrowsAsync<ArgumentNullException>(() => value.AuthenticateRequestAsync(null!));
    }

    [Fact]
    public async Task AddsInHeader()
    {
        // When using 'username' and 'password', the expected encoded auth
        // header is "Basic dXNlcm5hbWU6cGFzc3dvcmQ="


        var value = new BasicAuthenticationProvider("username", "password");
        var request = new RequestInformation {
            UrlTemplate = "https://localhost{?param1}",
        };
        await value.AuthenticateRequestAsync(request);


        Assert.True(request.Headers.TryGetValue("Authorization", out var authHeader));
        Assert.Single(authHeader);
        Assert.Equal("Basic dXNlcm5hbWU6cGFzc3dvcmQ=", authHeader.First());
    }

}