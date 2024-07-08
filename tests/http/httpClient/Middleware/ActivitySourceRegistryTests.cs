using System;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Xunit;

namespace Microsoft.Kiota.Http.HttpClientLibrary.Tests.Middleware.Registries
{
    public class ActivitySourceRegistryTests
    {
        [Fact]
        public void Defensive()
        {
            Assert.Throws<ArgumentNullException>(() => ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(""));
            Assert.Throws<ArgumentNullException>(() => ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource(null));
        }

        [Fact]
        public void CreatesNewInstanceOnFirstCallAndReturnsSameInstance()
        {
            // Act
            var activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource("sample source");
            Assert.NotNull(activitySource);

            var activitySource2 = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource("sample source");
            Assert.NotNull(activitySource);

            // They are the same instance
            Assert.Equal(activitySource, activitySource2);
            Assert.Equal("sample source", activitySource.Name);
            Assert.Equal("sample source", activitySource2.Name);
        }

        [Fact]
        public void CreatesDifferentInstances()
        {
            // Act
            var activitySource = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource("sample source");
            Assert.NotNull(activitySource);
            Assert.Equal("sample source", activitySource.Name);

            var activitySource2 = ActivitySourceRegistry.DefaultInstance.GetOrCreateActivitySource("sample source 2");
            Assert.NotNull(activitySource);
            Assert.Equal("sample source 2", activitySource2.Name);

            // They are not the same instance
            Assert.NotEqual(activitySource, activitySource2);
        }
    }
}
