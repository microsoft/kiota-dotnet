using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests
{
    public class RequestInformationTests
    {
        [Fact]
        public void SetUriExtractsQueryParameters()
        {
            // Arrange
            var testRequest = new RequestInformation()
            {
                HttpMethod = Method.GET,
                UrlTemplate = "http://localhost/{path}/me?foo={foo}"
            };
            // Act
            testRequest.QueryParameters.Add("foo", "bar");
            testRequest.PathParameters.Add("path", "baz");
            // Assert
            Assert.Equal("http://localhost/baz/me?foo=bar", testRequest.URI.ToString());
            Assert.NotEmpty(testRequest.QueryParameters);
            Assert.Equal("foo",testRequest.QueryParameters.First().Key);
            Assert.Equal("bar", testRequest.QueryParameters.First().Value.ToString());
        }
        [Fact]
        public void AddsAndRemovesRequestOptions()
        {
            // Arrange
            var testRequest = new RequestInformation()
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost")
            };
            var testRequestOption = new Mock<IRequestOption>().Object;
            Assert.Empty(testRequest.RequestOptions);
            // Act
            testRequest.AddRequestOptions(new IRequestOption[] {testRequestOption});
            // Assert
            Assert.NotEmpty(testRequest.RequestOptions);
            Assert.Equal(testRequestOption, testRequest.RequestOptions.First());

            // Act by removing the option
            testRequest.RemoveRequestOptions(testRequestOption);
            Assert.Empty(testRequest.RequestOptions);
        }
        [Fact]
        public void SetsSelectQueryParameters()
        {
            // Arrange as the request builders would
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "http://localhost/me{?%24select}"
            };
            Action<GetQueryParameters> q = x => x.Select = new[] { "id", "displayName" };
            var qParams = new GetQueryParameters();
            q.Invoke(qParams);

            // Act 
            requestInfo.AddQueryParameters(qParams);

            // Assert
            Assert.True(requestInfo.QueryParameters.ContainsKey("%24select"));
            Assert.False(requestInfo.QueryParameters.ContainsKey("select"));
            Assert.Equal("%24select",requestInfo.QueryParameters.First().Key);
        }
        [Fact]
        public void SetsPathParametersOfDateTimeOffsetType()
        {
            // Arrange as the request builders would
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "http://localhost/getDirectRoutingCalls(fromDateTime='{fromDateTime}',toDateTime='{toDateTime}')"
            };

            // Act
            var fromDateTime = new DateTimeOffset(2022, 8, 1, 0, 0, 0, TimeSpan.Zero);
            var toDateTime = new DateTimeOffset(2022, 8, 2, 0, 0, 0, TimeSpan.Zero);
            var pathParameters = new Dictionary<string, object>();
            pathParameters.Add("fromDateTime", fromDateTime);
            pathParameters.Add("toDateTime", toDateTime);

            requestInfo.PathParameters = pathParameters;

            // Assert
            Assert.Contains("fromDateTime='2022-08-01T00%3A00%3A00.0000000%2B00%3A00'", requestInfo.URI.OriginalString);
            Assert.Contains("toDateTime='2022-08-02T00%3A00%3A00.0000000%2B00%3A00'", requestInfo.URI.OriginalString);
        }
        [Fact]
        public void SetsPathParametersOfBooleanType()
        {
            // Arrange as the request builders would
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "http://localhost/users{?%24count}"
            };

            // Act
            var count = true;
            var pathParameters = new Dictionary<string, object>();
            pathParameters.Add("%24count", count);

            requestInfo.PathParameters = pathParameters;

            // Assert
            Assert.Contains("%24count=true", requestInfo.URI.OriginalString);
        }

        [Fact]
        public void ThrowsInvalidOperationExceptionWhenBaseUrlNotSet()
        {
            // Arrange as the request builders would
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}/users{?%24count}"
            };

            // Assert
            var exception = Assert.Throws<InvalidOperationException>(() => requestInfo.URI);
            Assert.Contains("baseurl", exception.Message);
        }

        [Fact]
        public void BuildsUrlOnProvidedBaseUrl()
        {
            // Arrange as the request builders would
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}/users{?%24count}"
            };

            // Act
            requestInfo.PathParameters = new Dictionary<string, object>() 
            {
                { "baseurl","http://localhost" }
            };

            // Assert
            Assert.Contains("http://localhost", requestInfo.URI.OriginalString);
        }

        [Fact]
        public void InitializeWithProxyBaseUrl()
        {
            var proxyUrl = "https://proxy.apisandbox.msdn.microsoft.com/svc?url=https://graph.microsoft.com/beta";

            // Arrange as the request builders would
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = "{+baseurl}/users{?%24count}"
            };

            // Act
            requestInfo.PathParameters = new Dictionary<string, object>()
            {
                { "baseurl", proxyUrl },
                { "%24count", true }
            };

            // Assert we can build urls based on a Proxy based base url
            Assert.Equal("https://proxy.apisandbox.msdn.microsoft.com/svc?url=https://graph.microsoft.com/beta/users?%24count=true", requestInfo.URI.OriginalString);
        }
    }

    /// <summary>The messages in a mailbox or folder. Read-only. Nullable.</summary>
    internal class GetQueryParameters
    {
        /// <summary>Select properties to be returned</summary>\
        [QueryParameter("%24select")]
        public string[] Select { get; set; }
        /// <summary>Include count of items</summary>
        [QueryParameter("%24count")]
        public bool? Count { get; set; }
        /// <summary>Expand related entities</summary>
        [QueryParameter("%24filter")]
        public string Filter { get; set; }
        /// <summary>Order items by property values</summary>
        [QueryParameter("%24orderby")]
        public string[] Orderby { get; set; }
        /// <summary>Search items by search phrases</summary>
        [QueryParameter("%24search")]
        public string Search { get; set; }
    }
}
