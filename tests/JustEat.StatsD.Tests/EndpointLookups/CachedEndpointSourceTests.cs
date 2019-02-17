using System;
using System.Net;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;

namespace JustEat.StatsD.EndpointLookups
{
    public static class CachedEndpointSourceTests
    {
        [Fact]
        public static void CachedValueIsReturnedFromInner()
        {
            var mockInner = new Mock<IEndPointSource>();
            mockInner.Setup(x => x.GetEndpoint()).Returns(MakeTestIpEndPoint());

            var cachedEndpoint = new CachedEndpointSource(mockInner.Object, TimeSpan.FromMinutes(5));

            var value = cachedEndpoint.GetEndpoint();
            value.ShouldNotBeNull();
            value.ShouldBe(MakeTestIpEndPoint());

            mockInner.Verify(x=> x.GetEndpoint(), Times.Exactly(1));
        }

        [Fact]
        public static void CachedValueIsReturnedOnce()
        {
            var mockInner = new Mock<IEndPointSource>();
            mockInner.Setup(x => x.GetEndpoint()).Returns(MakeTestIpEndPoint());

            var cachedEndpoint = new CachedEndpointSource(mockInner.Object, TimeSpan.FromMinutes(5));

            var value1 = cachedEndpoint.GetEndpoint();
            var value2 = cachedEndpoint.GetEndpoint();
            var value3 = cachedEndpoint.GetEndpoint();

            value1.ShouldNotBeNull();
            value1.ShouldBe(value2);
            value1.ShouldBe(value3);

            mockInner.Verify(x => x.GetEndpoint(), Times.Exactly(1));
        }

        [Fact]
        public static async Task CachedValueIsReturnedAgainAfterExpiry()
        {
            var mockInner = new Mock<IEndPointSource>();
            mockInner.Setup(x => x.GetEndpoint()).Returns(MakeTestIpEndPoint());

            var cachedEndpoint = new CachedEndpointSource(mockInner.Object, TimeSpan.FromSeconds(1));

            var value1 = cachedEndpoint.GetEndpoint();
            var value2 = cachedEndpoint.GetEndpoint();

            await Task.Delay(1500);

            var value3 = cachedEndpoint.GetEndpoint();
            var value4 = cachedEndpoint.GetEndpoint();

            mockInner.Verify(x => x.GetEndpoint(), Times.Exactly(2));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public static void ConstructorThrowsIfCacheDurationIsInvalid(long ticks)
        {
            // Arrange
            var inner = Mock.Of<IEndPointSource>();
            var cacheDuration = TimeSpan.FromTicks(ticks);

            // Act and Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>("cacheDuration", () => new CachedEndpointSource(inner, cacheDuration));

            exception.ActualValue.ShouldBe(cacheDuration);
        }

        [Fact]
        public static void ConstructorThrowsIfInnerIsNull()
        {
            // Arrange
            IEndPointSource inner = null;
            var cacheDuration = TimeSpan.FromHours(1);

            // Act and Assert
           Assert.Throws<ArgumentNullException>("inner", () => new CachedEndpointSource(inner, cacheDuration));
        }

        private static IPEndPoint MakeTestIpEndPoint()
        {
            return new IPEndPoint(new IPAddress(new byte[] { 1, 2, 3, 4 }), 8125);
        }
    }
}
