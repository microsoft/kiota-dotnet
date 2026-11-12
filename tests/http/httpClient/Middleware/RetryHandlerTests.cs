using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using Microsoft.Kiota.Http.HttpClientLibrary.Tests.Mocks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware
{
    public sealed class RetryHandlerTests : IDisposable
    {
        private readonly MockRedirectHandler _testHttpMessageHandler;
        private readonly RetryHandler _retryHandler;
        private readonly HttpMessageInvoker _invoker;
        private const string RetryAfter = "Retry-After";
        private const string RetryAttempt = "Retry-Attempt";

        public RetryHandlerTests()
        {
            _testHttpMessageHandler = new MockRedirectHandler();
            _retryHandler = new RetryHandler
            {
                InnerHandler = _testHttpMessageHandler
            };
            _invoker = new HttpMessageInvoker(_retryHandler);
        }

        public void Dispose()
        {
            _invoker.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void RetryHandlerConstructor()
        {
            // Act
            using RetryHandler retry = new RetryHandler();
            // Assert
            Assert.Null(retry.InnerHandler);
            Assert.NotNull(retry.RetryOption);
            Assert.Equal(RetryHandlerOption.DefaultMaxRetry, retry.RetryOption.MaxRetry);
            Assert.IsType<RetryHandler>(retry);
        }

        [Fact]
        public void RetryHandlerHttpMessageHandlerConstructor()
        {
            // Assert
            Assert.NotNull(_retryHandler.InnerHandler);
            Assert.NotNull(_retryHandler.RetryOption);
            Assert.Equal(RetryHandlerOption.DefaultMaxRetry, _retryHandler.RetryOption.MaxRetry);
            Assert.Equal(_retryHandler.InnerHandler, _testHttpMessageHandler);
            Assert.IsType<RetryHandler>(_retryHandler);
        }

        [Fact]
        public void RetryHandlerRetryOptionConstructor()
        {
            // Act
            using RetryHandler retry = new RetryHandler(new RetryHandlerOption { MaxRetry = 5, ShouldRetry = (_, _, _) => true });
            // Assert
            Assert.Null(retry.InnerHandler);
            Assert.NotNull(retry.RetryOption);
            Assert.Equal(5, retry.RetryOption.MaxRetry);
            Assert.IsType<RetryHandler>(retry);
        }

        [Fact]
        public void RetryHandlerOptionShouldThrowOnNegativeDelay()
        {
            Assert.Throws<InvalidOperationException>(() => new RetryHandlerOption { Delay = -1 });
        }

        [Fact]
        public void RetryHandlerOptionShouldThrowOnNegativeMaxRetry()
        {
            Assert.Throws<InvalidOperationException>(() => new RetryHandlerOption { MaxRetry = -1 });
        }

        [Fact]
        public void RetryHandlerOptionShouldThrowOnNegativeRetriesTimeLimit()
        {
            Assert.Throws<InvalidOperationException>(() => new RetryHandlerOption { RetriesTimeLimit = TimeSpan.FromSeconds(-1) });
        }

        [Fact]
        public void RetryHandlerOptionShouldAllowZeroMaxRetryAndRetriesTimeLimit()
        {
            var retryHandlerOption = new RetryHandlerOption
            {
                MaxRetry = 0,
                RetriesTimeLimit = TimeSpan.Zero
            };

            Assert.Equal(0, retryHandlerOption.MaxRetry);
            Assert.Equal(TimeSpan.Zero, retryHandlerOption.RetriesTimeLimit);
        }

        [Fact]
        public async Task OkStatusShouldPassThrough()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            var retryResponse = new HttpResponseMessage(HttpStatusCode.OK);
            _testHttpMessageHandler.SetHttpResponse(retryResponse);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.Same(response, retryResponse);
            Assert.NotNull(response.RequestMessage);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.False(response.RequestMessage.Headers.Contains(RetryAttempt), "The request add header wrong.");
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryWithAddRetryAttemptHeader(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            var retryResponse = new HttpResponseMessage(statusCode);
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.Same(response, response2);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage);
            Assert.NotNull(response.RequestMessage.Headers);
            Assert.True(response.RequestMessage.Headers.Contains(RetryAttempt));
            Assert.True(response.RequestMessage.Headers.TryGetValues(RetryAttempt, out var values));
            Assert.Single(values);
            Assert.Equal(values.First(), 1.ToString());
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryWithBuffedContent(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
            var retryResponse = new HttpResponseMessage(statusCode);
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.Same(response, response2);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.NotNull(response.RequestMessage.Content.Headers.ContentLength);
            Assert.Equal("Hello World", await response.RequestMessage.Content.ReadAsStringAsync(
#if NET5_0_OR_GREATER
                TestContext.Current.CancellationToken
#endif
            ));
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldNotRetryWithPostStreaming(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Test Content")
            };
            httpRequestMessage.Content.Headers.ContentLength = -1;
            var retryResponse = new HttpResponseMessage(statusCode);
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.NotEqual(response, response2);
            Assert.Same(response, retryResponse);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.NotNull(response.RequestMessage.Content.Headers.ContentLength);
            Assert.Equal(response.RequestMessage.Content.Headers.ContentLength, -1);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldNotRetryWithPutStreaming(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "http://example.org/foo")
            {
                Content = new StringContent("Test Content")
            };
            httpRequestMessage.Content.Headers.ContentLength = -1;
            var retryResponse = new HttpResponseMessage(statusCode);
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.NotEqual(response, response2);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.Same(response, retryResponse);
            Assert.NotNull(response.RequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.Equal(response.RequestMessage.Content.Headers.ContentLength, -1);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryWithQueryBufferedContent(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(new HttpMethod("QUERY"), "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
            var retryResponse = new HttpResponseMessage(statusCode);
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.Same(response, response2);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage);
            Assert.Equal(httpRequestMessage.Method, response.RequestMessage.Method);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.Equal("Hello World", await response.RequestMessage.Content.ReadAsStringAsync(
#if NET5_0_OR_GREATER
                TestContext.Current.CancellationToken
#endif
            ));
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldNotRetryWithQueryStreaming(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(new HttpMethod("QUERY"), "http://example.org/foo")
            {
                Content = new StringContent("Test Content")
            };
            httpRequestMessage.Content.Headers.ContentLength = -1;
            var retryResponse = new HttpResponseMessage(statusCode);
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.Same(response, retryResponse);
            Assert.Same(response.RequestMessage, httpRequestMessage);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ExceedMaxRetryShouldReturn(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            var retryResponse = new HttpResponseMessage(statusCode);
            var response2 = new HttpResponseMessage(statusCode);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            var retryHandler = new RetryHandler
            {
                InnerHandler = _testHttpMessageHandler,
                RetryOption = new RetryHandlerOption { Delay = 1 }
            };
            var invoker = new HttpMessageInvoker(retryHandler);
            // Act
            try
            {
                await invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            }
            catch(Exception exception)
            {
                // Assert
                Assert.IsType<AggregateException>(exception);
                var aggregateException = exception as AggregateException;
                Assert.StartsWith("Too many retries performed.", aggregateException!.Message);
                Assert.All(aggregateException.InnerExceptions, innerexception => Assert.IsType<ApiException>(innerexception));
                Assert.All(aggregateException.InnerExceptions, innerexception => Assert.True((innerexception as ApiException)!.ResponseStatusCode == (int)statusCode));
                Assert.False(httpRequestMessage.Headers.TryGetValues(RetryAttempt, out _), "Don't set Retry-Attempt Header");
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldDelayBasedOnRetryAfterHeaderWithSeconds(HttpStatusCode statusCode)
        {
            // Arrange
            var retryResponse = new HttpResponseMessage(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, 1.ToString());
            // Act
            await DelayTestWithMessage(retryResponse, 1, "Init");
            // Assert
            Assert.Equal("Init Work 1", Message);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldFallbackToExponentialBackOffForNegativeRetryAfterHeaderWithSeconds(HttpStatusCode statusCode)
        {
            // Arrange
            using var retryResponse = new HttpResponseMessage(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, (-1).ToString());

            // Act
            await RetryHandler.DelayAsync(retryResponse, 1, 1, out var delayInSeconds, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(2, delayInSeconds);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldDelayBasedOnRetryAfterHeaderWithHttpDate(HttpStatusCode statusCode)
        {
            // Arrange
            var retryResponse = new HttpResponseMessage(statusCode);
            var futureTime = DateTime.UtcNow + TimeSpan.FromSeconds(3);// 3 seconds from now
            var futureTimeString = futureTime.ToString(CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.InvariantCulture);
            Assert.Contains("GMT", futureTimeString); // http date always end in GMT according to the spec
            Assert.True(retryResponse.Headers.TryAddWithoutValidation(RetryAfter, futureTimeString));
            // Act
            await DelayTestWithMessage(retryResponse, 1, "Init");
            // Assert
            Assert.Equal("Init Work 1", Message);
        }

        [Theory]
        [InlineData(30)]
        [InlineData(90)]
        [InlineData(150)]
        [InlineData(3600)]
        [InlineData(86400)]
        public void ShouldUseTotalSecondsForRetryAfterHttpDate(int secondsFromNow)
        {
            // Arrange
            using var retryResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            var futureTime = DateTimeOffset.UtcNow.AddSeconds(secondsFromNow);
            // HTTP dates have whole-second precision.
            futureTime = futureTime.AddTicks(-(futureTime.Ticks % TimeSpan.TicksPerSecond));
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, futureTime.ToString("R", CultureInfo.InvariantCulture));

            // Act: a cancelled token lets us inspect the calculated delay without waiting.
            var before = DateTimeOffset.UtcNow;
            var delayTask = RetryHandler.DelayAsync(retryResponse, 0, 1, out var delayInSeconds, new CancellationToken(true));
            var after = DateTimeOffset.UtcNow;

            // Assert
            // TimeSpan.FromSeconds rounds to milliseconds on .NET Framework.
            const double roundingTolerance = 0.001;
            Assert.InRange(delayInSeconds,
                Math.Min((futureTime - after).TotalSeconds, RetryHandlerOption.MaxDelay) - roundingTolerance,
                Math.Min((futureTime - before).TotalSeconds, RetryHandlerOption.MaxDelay) + roundingTolerance);
            Assert.True(delayTask.IsCanceled);
        }

        [Theory]
        [InlineData(30)]
        [InlineData(90)]
        [InlineData(3600)]
        public void ShouldFallbackToExponentialBackOffForPastRetryAfterHttpDate(int secondsAgo)
        {
            // Arrange
            using var retryResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            var pastTime = DateTimeOffset.UtcNow.AddSeconds(-secondsAgo);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, pastTime.ToString("R", CultureInfo.InvariantCulture));

            // Act
            var delayTask = RetryHandler.DelayAsync(retryResponse, 2, 1, out var delayInSeconds, new CancellationToken(true));

            // Assert
            Assert.Equal(4, delayInSeconds);
            Assert.True(delayTask.IsCanceled);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldDelayBasedOnExponentialBackOff(HttpStatusCode statusCode)
        {
            // Arrange
            var retryResponse = new HttpResponseMessage(statusCode);
            var compareMessage = "Init Work ";

            for(int count = 0; count < 3; count++)
            {
                // Act
                await DelayTestWithMessage(retryResponse, count, "Init", 1);
                // Assert
                Assert.Equal(Message, compareMessage + count);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldReturnSameStatusCodeWhenDelayIsGreaterThanRetryTimeLimit(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
            var retryResponse = new HttpResponseMessage(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, 20.ToString());
            _retryHandler.RetryOption.RetriesTimeLimit = TimeSpan.FromSeconds(10);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.Same(response, retryResponse);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryBasedOnRetryAfterHeaderWithSeconds(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
            var retryResponse = new HttpResponseMessage(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, 5.ToString());
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.Same(response, response2);
            Assert.NotNull(response.RequestMessage);
            Assert.True(response.RequestMessage.Headers.TryGetValues(RetryAttempt, out var values), "Don't set Retry-Attempt Header");
            Assert.Single(values);
            Assert.Equal(values.First(), 1.ToString());
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryBasedOnRetryAfterHeaderWithHttpDate(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
            var retryResponse = new HttpResponseMessage(statusCode);
            var futureTime = DateTime.UtcNow + TimeSpan.FromSeconds(5);// 5 seconds from now
            var futureTimeString = futureTime.ToString(CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern);
            retryResponse.Headers.TryAddWithoutValidation(RetryAfter, futureTimeString);
            Assert.Contains("GMT", futureTimeString); // http date always end in GMT according to the spec
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.Same(response, response2);
            Assert.NotNull(response.RequestMessage);
            Assert.True(response.RequestMessage.Headers.TryGetValues(RetryAttempt, out var values), "Don't set Retry-Attempt Header");
            Assert.Single(values);
            Assert.Equal(values.First(), 1.ToString());
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
        }

        [Theory]
        [InlineData(1, HttpStatusCode.BadGateway, true)]
        [InlineData(2, HttpStatusCode.BadGateway, true)]
        [InlineData(3, HttpStatusCode.BadGateway, true)]
        [InlineData(4, HttpStatusCode.OK, false)]
        public async Task ShouldRetryBasedOnCustomShouldRetryDelegate(int expectedMaxRetry, HttpStatusCode expectedStatusCode, bool isExceptionExpected)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            Queue<HttpResponseMessage> httpResponseQueue = new(new HttpResponseMessage[]
            {
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.OK) { RequestMessage = request },
            });

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(() =>
                    {
                        HttpResponseMessage response;
                        try
                        {
                            response = httpResponseQueue.Dequeue();
                        }
                        catch
                        {
                            response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request };
                        }
                        return Task.FromResult(response);
                    })
                .Verifiable();

            RetryHandler retryHandler = new(new RetryHandlerOption()
            {
                ShouldRetry = (_, _, httpResponseMessage) => httpResponseMessage.StatusCode == HttpStatusCode.BadGateway,
                MaxRetry = expectedMaxRetry,
                Delay = 0
            })
            {
                InnerHandler = mockHttpMessageHandler.Object
            };

            HttpMessageInvoker httpMessageInvoker = new(retryHandler);

            // Act
            try
            {
                var response = await httpMessageInvoker.SendAsync(request, TestContext.Current.CancellationToken);

                Assert.False(isExceptionExpected);
                Assert.Equal(expectedStatusCode, response.StatusCode);
            }
            catch(Exception exception)
            {
                // Assert
                Assert.IsType<AggregateException>(exception);
                var aggregateException = exception as AggregateException;
                Assert.True(isExceptionExpected);
                Assert.StartsWith("Too many retries performed.", aggregateException!.Message);
                Assert.Equal(1 + expectedMaxRetry, aggregateException.InnerExceptions.Count);
                Assert.All(aggregateException.InnerExceptions, innerexception => Assert.Contains(expectedStatusCode.ToString(), innerexception.Message));
            }

            // Assert
            mockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Exactly(1 + expectedMaxRetry), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldNotRetryWithCustomShouldRetryDelegate(HttpStatusCode statusCode)
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo")
            {
                Content = new StringContent("Test Content")
            };
            httpRequestMessage.Content.Headers.ContentLength = -1;

            var retryResponse = new HttpResponseMessage(statusCode);
            var response2 = new HttpResponseMessage(HttpStatusCode.OK);
            this._testHttpMessageHandler.SetHttpResponse(retryResponse, response2);
            _retryHandler.RetryOption.ShouldRetry = (_, _, _) => false;
            // Act
            var response = await _invoker.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);
            // Assert
            Assert.NotEqual(response, response2);
            Assert.Same(response, retryResponse);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.NotNull(response.RequestMessage.Content.Headers.ContentLength);
            Assert.Equal(response.RequestMessage.Content.Headers.ContentLength, -1);
        }

        [Fact]
        public void RetryNonIdempotentRequestsWhenOutcomeUnknownShouldDefaultToTrue()
        {
            Assert.True(new RetryHandlerOption().RetryNonIdempotentRequestsWhenOutcomeUnknown);
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PATCH")]
        public async Task ShouldRetryNonIdempotentRequestOnGatewayTimeoutByDefault(string method)
        {
            // Arrange
            var sent = new List<HttpRequestMessage>();
            var invoker = CreateInvoker(new RetryHandlerOption { Delay = 0 }, sent, HttpStatusCode.GatewayTimeout, HttpStatusCode.OK);
            // Act
            var response = await invoker.SendAsync(CreateBufferedRequest(method), TestContext.Current.CancellationToken);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, sent.Count);
        }

        [Theory]
        [InlineData("POST", HttpStatusCode.GatewayTimeout)]
        [InlineData("PATCH", HttpStatusCode.GatewayTimeout)]
        [InlineData("post", HttpStatusCode.GatewayTimeout)]
        [InlineData("POST", HttpStatusCode.BadGateway)]
        [InlineData("POST", HttpStatusCode.InternalServerError)]
        public async Task ShouldNotRetryNonIdempotentRequestWhenOutcomeUnknownAndDisabled(string method, HttpStatusCode statusCode)
        {
            // Arrange
            var sent = new List<HttpRequestMessage>();
            var option = new RetryHandlerOption
            {
                Delay = 0,
                RetryNonIdempotentRequestsWhenOutcomeUnknown = false,
                // Accept server errors so that only the new option can stop the retry.
                ShouldRetry = (_, _, response) => (int)response.StatusCode >= 500 || (int)response.StatusCode == 429
            };
            var invoker = CreateInvoker(option, sent, statusCode, HttpStatusCode.OK);
            var request = CreateBufferedRequest(method);
            // Act
            var response = await invoker.SendAsync(request, TestContext.Current.CancellationToken);
            // Assert
            Assert.Equal(statusCode, response.StatusCode);
            Assert.Single(sent);
            Assert.Same(request, response.RequestMessage);
        }

        [Theory]
        [InlineData("POST", HttpStatusCode.ServiceUnavailable)]
        [InlineData("POST", (HttpStatusCode)429)]
        [InlineData("PATCH", HttpStatusCode.ServiceUnavailable)]
        [InlineData("PATCH", (HttpStatusCode)429)]
        public async Task ShouldRetryNonIdempotentRequestWhenNotProcessedAndDisabled(string method, HttpStatusCode statusCode)
        {
            // Arrange
            var sent = new List<HttpRequestMessage>();
            var option = new RetryHandlerOption { Delay = 0, RetryNonIdempotentRequestsWhenOutcomeUnknown = false };
            var invoker = CreateInvoker(option, sent, statusCode, HttpStatusCode.OK);
            // Act
            var response = await invoker.SendAsync(CreateBufferedRequest(method), TestContext.Current.CancellationToken);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, sent.Count);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("QUERY")]
        public async Task ShouldRetryIdempotentRequestOnGatewayTimeoutWhenDisabled(string method)
        {
            // Arrange
            var sent = new List<HttpRequestMessage>();
            var option = new RetryHandlerOption { Delay = 0, RetryNonIdempotentRequestsWhenOutcomeUnknown = false };
            var invoker = CreateInvoker(option, sent, HttpStatusCode.GatewayTimeout, HttpStatusCode.OK);
            // Act
            var response = await invoker.SendAsync(CreateBufferedRequest(method), TestContext.Current.CancellationToken);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, sent.Count);
        }

        [Fact]
        public async Task ShouldStopRetryingNonIdempotentRequestWhenALaterResponseLeavesOutcomeUnknown()
        {
            // Arrange
            var sent = new List<HttpRequestMessage>();
            var option = new RetryHandlerOption { Delay = 0, RetryNonIdempotentRequestsWhenOutcomeUnknown = false };
            var invoker = CreateInvoker(option, sent, HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout, HttpStatusCode.OK);
            // Act
            var response = await invoker.SendAsync(CreateBufferedRequest("POST"), TestContext.Current.CancellationToken);
            // Assert
            Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
            Assert.Equal(2, sent.Count);
        }

        [Fact]
        public async Task ShouldHonorRetryNonIdempotentRequestsWhenOutcomeUnknownFromRequestOption()
        {
            // Arrange
            var sent = new List<HttpRequestMessage>();
            var invoker = CreateInvoker(new RetryHandlerOption { Delay = 0 }, sent, HttpStatusCode.GatewayTimeout, HttpStatusCode.OK);
            var request = CreateBufferedRequest("POST");
            var requestOption = new RetryHandlerOption { Delay = 0, RetryNonIdempotentRequestsWhenOutcomeUnknown = false };
#if NET5_0_OR_GREATER
            request.Options.Set(new HttpRequestOptionsKey<RetryHandlerOption>(typeof(RetryHandlerOption).FullName!), requestOption);
#else
            request.Properties.Add(typeof(RetryHandlerOption).FullName!, requestOption);
#endif
            // Act
            var response = await invoker.SendAsync(request, TestContext.Current.CancellationToken);
            // Assert
            Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
            Assert.Single(sent);
        }

        [Fact]
        public async Task ShouldRetryRedirectedGetWhenNonIdempotentRetriesDisabled()
        {
            // Arrange: a POST that a redirect turned into a GET, which then got a 504
            var option = new RetryHandlerOption { Delay = 0, RetryNonIdempotentRequestsWhenOutcomeUnknown = false };
            var sent = new List<HttpRequestMessage>();
            var responses = new Queue<HttpStatusCode>(new[] { HttpStatusCode.GatewayTimeout, HttpStatusCode.OK });
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken _) =>
                {
                    sent.Add(request);
                    var sentRequest = request.Method == HttpMethod.Post ? new HttpRequestMessage(HttpMethod.Get, "http://example.org/bar") : request;
                    return Task.FromResult(new HttpResponseMessage(responses.Dequeue()) { RequestMessage = sentRequest });
                });
            var invoker = new HttpMessageInvoker(new RetryHandler(option) { InnerHandler = mockHttpMessageHandler.Object });

            // Act
            var response = await invoker.SendAsync(CreateBufferedRequest("POST"), TestContext.Current.CancellationToken);

            // Assert: the GET is resent, not the POST
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, sent.Count);
            Assert.Equal(HttpMethod.Get, sent[1].Method);
        }

        private static HttpRequestMessage CreateBufferedRequest(string method)
        {
            return new HttpRequestMessage(new HttpMethod(method), "http://example.org/foo")
            {
                Content = new StringContent("Hello World")
            };
        }

        private static HttpMessageInvoker CreateInvoker(RetryHandlerOption option, List<HttpRequestMessage> sent, params HttpStatusCode[] statusCodes)
        {
            var responses = new Queue<HttpStatusCode>(statusCodes);
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns((HttpRequestMessage request, CancellationToken _) =>
                {
                    sent.Add(request);
                    return Task.FromResult(new HttpResponseMessage(responses.Dequeue()) { RequestMessage = request });
                });
            return new HttpMessageInvoker(new RetryHandler(option) { InnerHandler = mockHttpMessageHandler.Object });
        }

        private async Task DelayTestWithMessage(HttpResponseMessage response, int count, string message, int delay = RetryHandlerOption.MaxDelay)
        {
            Message = message;
            await Task.Run(async () =>
            {
                await RetryHandler.DelayAsync(response, count, delay, out _, TestContext.Current.CancellationToken);
                Message += " Work " + count;
            });
        }

        private string? Message
        {
            get;
            set;
        }
    }
}
