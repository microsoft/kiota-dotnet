using System;
using System.Linq;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests
{
    public class QueryParametersBaseTests
    {
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
            qParams.AddQueryParameters(requestInfo.QueryParameters);

            // Assert
            Assert.True(requestInfo.QueryParameters.ContainsKey("%24select"));
            Assert.False(requestInfo.QueryParameters.ContainsKey("select"));
            Assert.Equal("%24select",requestInfo.QueryParameters.First().Key);
        }
    }

    /// <summary>The messages in a mailbox or folder. Read-only. Nullable.</summary>
    internal class GetQueryParameters : QueryParametersBase
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
