using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Kiota.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Authentication.Azure.Tests;

public class AzureIdentityAuthenticationProviderTests
{
    [Fact]
    public void ConstructorThrowsArgumentNullExceptionOnNullTokenCredential()
    {
        // Arrange
        var exception = Assert.Throws<ArgumentNullException>(() => new AzureIdentityAccessTokenProvider(null!, null));

        // Assert
        Assert.Equal("credential", exception.ParamName);
    }

    [Theory]
    [InlineData("https://localhost", "")]
    [InlineData("https://graph.microsoft.com", "token")]
    [InlineData("https://graph.microsoft.com/v1.0/me", "token")]
    public async Task GetAuthorizationTokenAsyncGetsToken(string url, string expectedToken)
    {
        // Arrange
        var uri = new Uri(url);
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential.Setup(credential => credential.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>())).Returns(new ValueTask<AccessToken>(new AccessToken(expectedToken, DateTimeOffset.Now)));
        var azureIdentityAuthenticationProvider = new AzureIdentityAccessTokenProvider(mockTokenCredential.Object);

        // Act
        var token = await azureIdentityAuthenticationProvider.GetAuthorizationTokenAsync(uri, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedToken, token);
        mockTokenCredential.Verify(x => x.GetTokenAsync(It.Is<TokenRequestContext>(t =>
                                                                            t.Scopes.Any(s => $"{uri.Scheme}://{uri.Host}/.default".Equals(s, StringComparison.OrdinalIgnoreCase))), It.IsAny<CancellationToken>()));
    }

    [Theory]
    [InlineData("https://localhost", "")]
    [InlineData("https://graph.microsoft.com", "token")]
    [InlineData("https://graph.microsoft.com/v1.0/me", "token")]
    public async Task AuthenticateRequestAsyncSetsBearerHeader(string url, string expectedToken)
    {
        // Arrange
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential.Setup(credential => credential.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>())).Returns(new ValueTask<AccessToken>(new AccessToken(expectedToken, DateTimeOffset.Now)));
        var azureIdentityAuthenticationProvider = new AzureIdentityAuthenticationProvider(mockTokenCredential.Object, scopes: "User.Read");
        var testRequest = new RequestInformation()
        {
            HttpMethod = Method.GET,
            URI = new Uri(url)
        };
        Assert.Empty(testRequest.Headers); // header collection is empty

        // Act
        await azureIdentityAuthenticationProvider.AuthenticateRequestAsync(testRequest, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        if(string.IsNullOrEmpty(expectedToken))
        {
            Assert.Empty(testRequest.Headers); // header collection is still empty
        }
        else
        {
            Assert.NotEmpty(testRequest.Headers); // header collection is no longer empty
            Assert.Equal("Authorization", testRequest.Headers.First().Key); // First element is Auth header
            Assert.Equal($"Bearer {expectedToken}", testRequest.Headers["Authorization"].First()); // First element is Auth header
        }
    }

    [Fact]
    public async Task GetAuthorizationTokenAsyncThrowsExcpetionForNonHTTPsUrl()
    {
        // Arrange
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential.Setup(credential => credential.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>())).Returns(new ValueTask<AccessToken>(new AccessToken(string.Empty, DateTimeOffset.Now)));
        var azureIdentityAuthenticationProvider = new AzureIdentityAccessTokenProvider(mockTokenCredential.Object);

        var nonHttpsUrl = "http://graph.microsoft.com";

        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => azureIdentityAuthenticationProvider.GetAuthorizationTokenAsync(new Uri(nonHttpsUrl), cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("Only https is supported", exception.Message);
    }

    [Theory]
    [InlineData("http://localhost/test")]
    [InlineData("http://localhost:8080/test")]
    [InlineData("http://127.0.0.1:8080/test")]
    [InlineData("http://127.0.0.1/test")]
    public async Task GetAuthorizationTokenAsyncDoesNotThrowsExcpetionForNonHTTPsUrlIfLocalHost(string nonHttpsUrl)
    {
        // Arrange
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential.Setup(credential => credential.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>())).Returns(new ValueTask<AccessToken>(new AccessToken(string.Empty, DateTimeOffset.Now)));
        var azureIdentityAuthenticationProvider = new AzureIdentityAccessTokenProvider(mockTokenCredential.Object);

        // Assert
        var token = await azureIdentityAuthenticationProvider.GetAuthorizationTokenAsync(new Uri(nonHttpsUrl), cancellationToken: TestContext.Current.CancellationToken);
        Assert.Empty(token);
    }
    [Fact]
    public async Task AddsClaimsToTheTokenContext()
    {
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential.Setup(credential => credential.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
                            .Returns<TokenRequestContext, CancellationToken>((context, cToken) =>
                            {
                                Assert.NotNull(context.Claims);
                                return new ValueTask<AccessToken>(new AccessToken(string.Empty, DateTimeOffset.Now));
                            });
        var azureIdentityAuthenticationProvider = new AzureIdentityAuthenticationProvider(mockTokenCredential.Object, scopes: "User.Read");
        var testRequest = new RequestInformation()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://graph.microsoft.com/v1.0/me")
        };
        Assert.Empty(testRequest.Headers); // header collection is empty

        // Act
        await azureIdentityAuthenticationProvider.AuthenticateRequestAsync(testRequest, new() { { "claims", "eyJhY2Nlc3NfdG9rZW4iOnsibmJmIjp7ImVzc2VudGlhbCI6dHJ1ZSwgInZhbHVlIjoiMTY1MjgxMzUwOCJ9fX0=" } }, TestContext.Current.CancellationToken);
        mockTokenCredential.Verify(x => x.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TokenRequestContextFactorySetsTenantIdDynamically()
    {
        TokenRequestContext? capturedContext = null;
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .Callback<TokenRequestContext, CancellationToken>((ctx, _) => capturedContext = ctx)
            .Returns(new ValueTask<AccessToken>(new AccessToken("tenant-token", DateTimeOffset.UtcNow)));

        var provider = new AzureIdentityAccessTokenProvider(
            mockTokenCredential.Object,
            true,
            (_, authCtx, defaultCtx) =>
            {
                var tenantId = authCtx != null && authCtx.TryGetValue("tenantId", out var t) ? t as string : null;
                return new TokenRequestContext(defaultCtx.Scopes, claims: defaultCtx.Claims, tenantId: tenantId, isCaeEnabled: defaultCtx.IsCaeEnabled);
            },
            "User.Read");

        var token = await provider.GetAuthorizationTokenAsync(
            new Uri("https://graph.microsoft.com/v1.0/me"),
            new Dictionary<string, object> { { "tenantId", "tenant-123" } },
            TestContext.Current.CancellationToken);

        Assert.Equal("tenant-token", token);
        Assert.NotNull(capturedContext);
        Assert.Equal("tenant-123", capturedContext!.Value.TenantId);
        Assert.Equal(new[] { "User.Read" }, capturedContext.Value.Scopes);
    }

    [Fact]
    public async Task AuthenticationProviderForwardsTokenRequestContextFactoryOnAuthenticateRequestAsync()
    {
        TokenRequestContext? capturedContext = null;
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .Callback<TokenRequestContext, CancellationToken>((ctx, _) => capturedContext = ctx)
            .Returns(new ValueTask<AccessToken>(new AccessToken("auth-provider-token", DateTimeOffset.UtcNow)));

        var authProvider = new AzureIdentityAuthenticationProvider(
            mockTokenCredential.Object,
            true,
            (_, authCtx, defaultCtx) =>
            {
                var tenantId = authCtx != null && authCtx.TryGetValue("tenantId", out var t) ? t as string : null;
                return new TokenRequestContext(defaultCtx.Scopes, claims: defaultCtx.Claims, tenantId: tenantId, isCaeEnabled: defaultCtx.IsCaeEnabled);
            },
            "Mail.Read");

        var request = new RequestInformation
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://graph.microsoft.com/v1.0/me/messages")
        };

        await authProvider.AuthenticateRequestAsync(
            request,
            new Dictionary<string, object> { { "tenantId", "contoso-tenant" } },
            TestContext.Current.CancellationToken);

        Assert.Equal("Bearer auth-provider-token", request.Headers["Authorization"].First());
        Assert.NotNull(capturedContext);
        Assert.Equal("contoso-tenant", capturedContext!.Value.TenantId);
        Assert.Equal(new[] { "Mail.Read" }, capturedContext.Value.Scopes);
    }

    [Fact]
    public async Task TokenRequestContextFactoryReceivesDecodedClaimsDefaultScopesAndIsCaeEnabled()
    {
        TokenRequestContext? capturedDefault = null;
        TokenRequestContext? capturedFinal = null;
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .Callback<TokenRequestContext, CancellationToken>((ctx, _) => capturedFinal = ctx)
            .Returns(new ValueTask<AccessToken>(new AccessToken("token", DateTimeOffset.UtcNow)));

        var provider = new AzureIdentityAccessTokenProvider(
            mockTokenCredential.Object,
            false,
            (_, _, defaultCtx) =>
            {
                capturedDefault = defaultCtx;
                return new TokenRequestContext(defaultCtx.Scopes, claims: defaultCtx.Claims, tenantId: "t-cae", isCaeEnabled: defaultCtx.IsCaeEnabled);
            });

        var authContext = new Dictionary<string, object>
        {
            { "claims", "eyJhY2Nlc3NfdG9rZW4iOnsibmJmIjp7ImVzc2VudGlhbCI6dHJ1ZX19fQ==" }
        };

        await provider.GetAuthorizationTokenAsync(
            new Uri("https://graph.microsoft.com/v1.0/users"),
            authContext,
            TestContext.Current.CancellationToken);

        Assert.NotNull(capturedDefault);
        Assert.Equal(new[] { "https://graph.microsoft.com/.default" }, capturedDefault!.Value.Scopes);
        Assert.Equal("{\"access_token\":{\"nbf\":{\"essential\":true}}}", capturedDefault.Value.Claims);
        Assert.False(capturedDefault.Value.IsCaeEnabled);
        Assert.NotNull(capturedFinal);
        Assert.Equal("t-cae", capturedFinal!.Value.TenantId);
        Assert.Equal("{\"access_token\":{\"nbf\":{\"essential\":true}}}", capturedFinal.Value.Claims);
        Assert.False(capturedFinal.Value.IsCaeEnabled);
    }

    [Fact]
    public async Task TokenRequestContextFactorySetsProofOfPossessionProperties()
    {
        TokenRequestContext? capturedContext = null;
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .Callback<TokenRequestContext, CancellationToken>((ctx, _) => capturedContext = ctx)
            .Returns(new ValueTask<AccessToken>(new AccessToken("pop-token", DateTimeOffset.UtcNow)));

        var provider = new AzureIdentityAccessTokenProvider(
            mockTokenCredential.Object,
            true,
            (_, authCtx, defaultCtx) => new TokenRequestContext(
                defaultCtx.Scopes,
                claims: defaultCtx.Claims,
                isCaeEnabled: defaultCtx.IsCaeEnabled,
                isProofOfPossessionEnabled: true,
                proofOfPossessionNonce: authCtx?["nonce"] as string));

        var token = await provider.GetAuthorizationTokenAsync(
            new Uri("https://graph.microsoft.com/v1.0/me"),
            new Dictionary<string, object> { { "nonce", "sample-pop-nonce" } },
            TestContext.Current.CancellationToken);

        Assert.Equal("pop-token", token);
        Assert.NotNull(capturedContext);
        Assert.True(capturedContext!.Value.IsProofOfPossessionEnabled);
        Assert.Equal("sample-pop-nonce", capturedContext.Value.ProofOfPossessionNonce);
    }

    [Fact]
    public async Task TokenRequestContextFactoryReceivesUriAndCanOverrideScopes()
    {
        Uri? capturedUri = null;
        TokenRequestContext? capturedContext = null;
        var mockTokenCredential = new Mock<TokenCredential>();
        mockTokenCredential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .Callback<TokenRequestContext, CancellationToken>((ctx, _) => capturedContext = ctx)
            .Returns(new ValueTask<AccessToken>(new AccessToken("scoped-token", DateTimeOffset.UtcNow)));

        var provider = new AzureIdentityAccessTokenProvider(
            mockTokenCredential.Object,
            true,
            (uri, _, defaultCtx) =>
            {
                capturedUri = uri;
                var overrideScopes = uri.AbsolutePath.StartsWith("/admin", StringComparison.OrdinalIgnoreCase)
                    ? new[] { "Directory.ReadWrite.All" }
                    : defaultCtx.Scopes;
                return new TokenRequestContext(overrideScopes, claims: defaultCtx.Claims, isCaeEnabled: defaultCtx.IsCaeEnabled);
            },
            "User.Read");

        var targetUri = new Uri("https://graph.microsoft.com/admin/settings");
        var token = await provider.GetAuthorizationTokenAsync(targetUri, null, TestContext.Current.CancellationToken);

        Assert.Equal("scoped-token", token);
        Assert.Equal(targetUri, capturedUri);
        Assert.NotNull(capturedContext);
        Assert.Equal(new[] { "Directory.ReadWrite.All" }, capturedContext!.Value.Scopes);
    }

    [Fact]
    public async Task TokenRequestContextFactoryIsNotInvokedWhenUrlValidationFails()
    {
        var factoryCalled = false;
        var mockTokenCredential = new Mock<TokenCredential>();
        var provider = new AzureIdentityAccessTokenProvider(
            mockTokenCredential.Object,
            true,
            (_, _, defaultCtx) =>
            {
                factoryCalled = true;
                return defaultCtx;
            });
        provider.AllowedHostsValidator.AllowedHosts = new[] { "graph.microsoft.com" };

        var disallowedHostToken = await provider.GetAuthorizationTokenAsync(
            new Uri("https://contoso.example.com/v1.0/me"),
            null,
            TestContext.Current.CancellationToken);

        Assert.Empty(disallowedHostToken);
        Assert.False(factoryCalled);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            provider.GetAuthorizationTokenAsync(new Uri("http://graph.microsoft.com/v1.0/me"), null, TestContext.Current.CancellationToken));
        Assert.False(factoryCalled);
        mockTokenCredential.Verify(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
