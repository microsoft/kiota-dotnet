using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Authentication;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests;
public class AuthenticationTests
{
    [Fact]
    public async Task AnonymousAuthenticationProviderReturnsSameRequestAsync()
    {
        // Arrange
        var anonymousAuthenticationProvider = new AnonymousAuthenticationProvider();
        var testRequest = new RequestInformation()
        {
            HttpMethod = Method.GET,
            URI = new Uri("http://localhost")
        };
        Assert.Empty(testRequest.Headers.Keys); // header collection is empty

        // Act
        await anonymousAuthenticationProvider.AuthenticateRequestAsync(testRequest);

        // Assert
        Assert.Empty(testRequest.Headers.Keys); // header collection is still empty

    }

    [Fact]
    public async Task BaseBearerTokenAuthenticationProviderSetsBearerHeader()
    {
        // Arrange
        var expectedToken = "token";
        var mockAccessTokenProvider = new Mock<IAccessTokenProvider>();
        mockAccessTokenProvider.Setup(authProvider => authProvider.GetAuthorizationTokenAsync(It.IsAny<Uri>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedToken));
        var testAuthProvider = new BaseBearerTokenAuthenticationProvider(mockAccessTokenProvider.Object);
        var testRequest = new RequestInformation()
        {
            HttpMethod = Method.GET,
            URI = new Uri("http://localhost")
        };
        Assert.Empty(testRequest.Headers.Keys); // header collection is empty

        // Act
        await testAuthProvider.AuthenticateRequestAsync(testRequest);

        // Assert
        Assert.NotEmpty(testRequest.Headers.Keys); // header collection is longer empty
        Assert.True(testRequest.Headers.ContainsKey("Authorization")); // First element is Auth header
        Assert.Equal($"Bearer {expectedToken}", testRequest.Headers["Authorization"].First()); // First element is Auth header
    }

    [Theory]
    [InlineData("https://graph.microsoft.com", true)]// PASS
    [InlineData("https://graph.microsoft.us/v1.0/me", true)]// PASS as we don't look at the path segment
    [InlineData("https://test.microsoft.com", false)]// Fail
    [InlineData("https://grAph.MicrosofT.com", true)] // PASS since we don't care about case
    [InlineData("https://developer.microsoft.com", false)] // Failed
    public void AllowedHostValidatorValidatesUrls(string urlToTest, bool expectedResult)
    {
        // Test through the constructor
        // Arrange
        var allowList = new[] { "graph.microsoft.com", "graph.microsoft.us" };
        var validator = new AllowedHostsValidator(allowList);

        // Act 
        var validationResult = validator.IsUrlHostValid(new Uri(urlToTest));

        // Assert
        Assert.Equal(expectedResult, validationResult);
        Assert.Contains(allowList[0], validator.AllowedHosts);
        Assert.Contains(allowList[1], validator.AllowedHosts);


        // Test through the setter
        // Arrange
        var emptyValidator = new AllowedHostsValidator
        {
            AllowedHosts = allowList // set the validator through the property
        };

        // Act 
        var emptyValidatorResult = emptyValidator.IsUrlHostValid(new Uri(urlToTest));

        // Assert
        Assert.Equal(emptyValidatorResult, validationResult);
        Assert.Contains(allowList[0], emptyValidator.AllowedHosts);
        Assert.Contains(allowList[1], emptyValidator.AllowedHosts);
    }


    [Theory]
    [InlineData("https://graph.microsoft.com")]// PASS
    [InlineData("https://graph.microsoft.us/v1.0/me")]// PASS
    [InlineData("https://test.microsoft.com")]// PASS
    [InlineData("https://grAph.MicrosofT.com")] // PASS
    [InlineData("https://developer.microsoft.com")] // PASS
    public void AllowedHostValidatorAllowsAllUrls(string urlToTest)
    {
        // Test through the constructor
        // Arrange
        var validator = new AllowedHostsValidator();

        // Act 
        var validationResult = validator.IsUrlHostValid(new Uri(urlToTest));

        // Assert
        Assert.True(validationResult);
        Assert.Empty(validator.AllowedHosts);
    }

    [Theory]
    [InlineData("https://graph.microsoft.com")] // https
    [InlineData("http://graph.microsoft.us")] // http
    [InlineData("HTTPS://TEST.MICROSOFT.COM")] // https with upperCase
    [InlineData("http://TEST.MICROSOFT.COM")] // http with upperCase
    [InlineData("http://developer.microsoft.com,graph.microsoft.com")] // a valid and an invalid together
    public void AllowedHostValidatorThrowsArgumentExceptionOnNonValidHost(string commaSeparatedHosts)
    {
        // Test through the constructor
        // Arrange
        var urlStrings = commaSeparatedHosts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        // Assert constructor throws
        var exception = Assert.Throws<ArgumentException>(() => new AllowedHostsValidator(urlStrings));
        Assert.Equal("host should not contain http or https prefix", exception.Message);
        // Assert setter throws
        var validator = new AllowedHostsValidator();
        Assert.Throws<ArgumentException>(() => validator.AllowedHosts = urlStrings);
        Assert.Equal("host should not contain http or https prefix", exception.Message);
    }

    [Fact]
    public async Task BaseBearerTokenAuthenticationProviderUsesConfigureAwaitFalse()
    {
        // Arrange
        var expectedToken = "test-token";
        var mockAccessTokenProvider = new Mock<IAccessTokenProvider>();
        var tokenTask = new TaskCompletionSource<string>();

        // Setup mock to return a task that we can control
        mockAccessTokenProvider.Setup(provider => provider.GetAuthorizationTokenAsync(
            It.IsAny<Uri>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<CancellationToken>()))
            .Returns(tokenTask.Task);

        var authProvider = new BaseBearerTokenAuthenticationProvider(mockAccessTokenProvider.Object);
        var request = new RequestInformation()
        {
            HttpMethod = Method.GET,
            URI = new Uri("https://example.com")
        };

        // Act - start the authentication task
        var authTask = authProvider.AuthenticateRequestAsync(request);

        // Complete the token task to allow authentication to complete
        tokenTask.SetResult(expectedToken);

        // Wait for authentication to complete
        await authTask;

        // Assert
        Assert.True(request.Headers.ContainsKey("Authorization"));
        Assert.Equal($"Bearer {expectedToken}", request.Headers["Authorization"].First());

        // Verify the mock was called correctly
        mockAccessTokenProvider.Verify(provider => provider.GetAuthorizationTokenAsync(
            It.IsAny<Uri>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
