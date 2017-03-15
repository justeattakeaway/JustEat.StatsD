using System.Net;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;

namespace JustEat.StatsD.EndpointLookups
{
    public static class CachedIpEndpointSourceTests
    {
        [Fact]
        public static void CachedValueIsReturnedFromInner()
        {
            var mockInner = new Mock<IPEndPointSource>();
            mockInner.SetupGet(x => x.Endpoint).Returns(MakeTestIpEndPoint());

            var cachedEndpoint = new CachedIpEndpointSource(mockInner.Object, 1234);

            var value = cachedEndpoint.Endpoint;
            value.ShouldNotBeNull();
            value.ShouldBe(MakeTestIpEndPoint());

            mockInner.VerifyGet(x=> x.Endpoint, Times.Exactly(1));
        }

        [Fact]
        public static void CachedValueIsReturnedOnce()
        {
            var mockInner = new Mock<IPEndPointSource>();
            mockInner.SetupGet(x => x.Endpoint).Returns(MakeTestIpEndPoint());

            var cachedEndpoint = new CachedIpEndpointSource(mockInner.Object, 1234);

            var value1 = cachedEndpoint.Endpoint;
            var value2 = cachedEndpoint.Endpoint;
            var value3 = cachedEndpoint.Endpoint;

            value1.ShouldNotBeNull();
            value1.ShouldBe(value2);
            value1.ShouldBe(value3);

            mockInner.VerifyGet(x => x.Endpoint, Times.Exactly(1));
        }

        [Fact]
        public static async Task CachedValueIsReturnedAgainAfterExpiry()
        {
            var mockInner = new Mock<IPEndPointSource>();
            mockInner.SetupGet(x => x.Endpoint).Returns(MakeTestIpEndPoint());

            var cachedEndpoint = new CachedIpEndpointSource(mockInner.Object, 1);

            var value1 = cachedEndpoint.Endpoint;
            var value2 = cachedEndpoint.Endpoint;

            await Task.Delay(1500);

            var value3 = cachedEndpoint.Endpoint;
            var value4 = cachedEndpoint.Endpoint;

            mockInner.VerifyGet(x => x.Endpoint, Times.Exactly(2));
        }

        private static IPEndPoint MakeTestIpEndPoint()
        {
            return new IPEndPoint(new IPAddress(new byte[] { 1, 2, 3, 4 }), 8125);
        }
    }
}
