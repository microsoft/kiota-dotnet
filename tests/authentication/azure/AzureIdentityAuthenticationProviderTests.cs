using System;
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
        var token = await azureIdentityAuthenticationProvider.GetAuthorizationTokenAsync(uri);

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
        await azureIdentityAuthenticationProvider.AuthenticateRequestAsync(testRequest);

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
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => azureIdentityAuthenticationProvider.GetAuthorizationTokenAsync(new Uri(nonHttpsUrl)));
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
        var token = await azureIdentityAuthenticationProvider.GetAuthorizationTokenAsync(new Uri(nonHttpsUrl));
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
        await azureIdentityAuthenticationProvider.AuthenticateRequestAsync(testRequest, new() { { "claims", "eyJhY2Nlc3NfdG9rZW4iOnsibmJmIjp7ImVzc2VudGlhbCI6dHJ1ZSwgInZhbHVlIjoiMTY1MjgxMzUwOCJ9fX0=" } });
        mockTokenCredential.Verify(x => x.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
