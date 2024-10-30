using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware
{
    public class AuthorizationHandlerTests : IDisposable
    {
        private readonly MockRedirectHandler _testHttpMessageHandler;
        private readonly string _expectedAccessToken = "token";

        private readonly string _expectedAccessTokenAfterCAE = "token2";
        private AuthorizationHandler _authorizationHandler;
        private readonly BaseBearerTokenAuthenticationProvider _authenticationProvider;
        private readonly HttpMessageInvoker _invoker;

        private readonly string _claimsChallengeHeaderValue = "authorization_uri=\"https://login.windows.net/common/oauth2/authorize\","
                + "error=\"insufficient_claims\","
                + "claims=\"eyJhY2Nlc3NfdG9rZW4iOnsibmJmIjp7ImVzc2VudGlhbCI6dHJ1ZSwgInZhbHVlIjoiMTYwNDEwNjY1MSJ9fX0=\"";

        public AuthorizationHandlerTests()
        {
            this._testHttpMessageHandler = new MockRedirectHandler();
            var mockAccessTokenProvider = new Mock<IAccessTokenProvider>();
            mockAccessTokenProvider.SetupSequence(x => x.GetAuthorizationTokenAsync(
                It.IsAny<Uri>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()
            ).Result).Returns(_expectedAccessToken)
            .Returns(_expectedAccessTokenAfterCAE);

            mockAccessTokenProvider.Setup(x => x.AllowedHostsValidator).Returns(
                new AllowedHostsValidator(new List<string> { "graph.microsoft.com" })
            );
            var mockAuthenticationProvider = new Mock<BaseBearerTokenAuthenticationProvider>(mockAccessTokenProvider.Object);
            this._authenticationProvider = mockAuthenticationProvider.Object;
            this._authorizationHandler = new AuthorizationHandler(_authenticationProvider)
            {
                InnerHandler = this._testHttpMessageHandler
            };

            this._invoker = new HttpMessageInvoker(this._authorizationHandler);
        }

        public void Dispose()
        {
            this._invoker.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task AuthorizationHandlerShouldAddAuthHeaderIfNotPresent()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/me");

            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(httpResponse);// set the mock response
            // Act
            HttpResponseMessage response = await this._invoker.SendAsync(httpRequestMessage, new CancellationToken());
            // Assert
            Assert.NotNull(response.RequestMessage);
            Assert.True(response.RequestMessage.Headers.Contains("Authorization"));
            Assert.True(response.RequestMessage.Headers.GetValues("Authorization").Count() == 1);
            Assert.Equal($"Bearer {_expectedAccessToken}", response.RequestMessage.Headers.GetValues("Authorization").First());
        }

        [Fact]
        public async Task AuthorizationHandlerShouldNotAddAuthHeaderIfPresent()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/me");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "existing");

            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(httpResponse);// set the mock response

            // Act
            HttpResponseMessage response = await this._invoker.SendAsync(httpRequestMessage, new CancellationToken());

            // Assert
            Assert.NotNull(response.RequestMessage);
            Assert.True(response.RequestMessage.Headers.Contains("Authorization"));
            Assert.True(response.RequestMessage.Headers.GetValues("Authorization").Count() == 1);
            Assert.Equal($"Bearer existing", response.RequestMessage.Headers.GetValues("Authorization").First());
        }

        [Fact]
        public async Task AuthorizationHandlerShouldAttemptCAEClaimsChallenge()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com");
            httpRequestMessage.Content = new ByteArrayContent(Encoding.UTF8.GetBytes("test"));

            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            httpResponse.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer", _claimsChallengeHeaderValue));

            this._testHttpMessageHandler.SetHttpResponse(httpResponse, new HttpResponseMessage(HttpStatusCode.OK));// set the mock response

            // Act
            HttpResponseMessage response = await this._invoker.SendAsync(httpRequestMessage, new CancellationToken());

            // Assert
            Assert.NotNull(response.RequestMessage);
            Assert.True(response.RequestMessage.Headers.Contains("Authorization"));
            Assert.True(response.RequestMessage.Headers.GetValues("Authorization").Count() == 1);
            Assert.Equal($"Bearer {_expectedAccessTokenAfterCAE}", response.RequestMessage.Headers.GetValues("Authorization").First());
            Assert.Equal("test", await response.RequestMessage.Content!.ReadAsStringAsync());
        }
    }
}
