using System.Net;
using Shouldly;
using Xunit;

namespace JustEat.StatsD.EndpointLookups
{
    public static class SimpleIpEndpointTests
    {
        [Fact]
        public static void CanHoldValue()
        {
            var wrapped = new SimpleIpEndpoint(MakeTestIpEndPoint());

            var expected = MakeTestIpEndPoint();
            wrapped.Endpoint.ShouldBe(expected);
        }

        [Fact]
        public static void ValueIsConsistent()
        {
            var wrapped = new SimpleIpEndpoint(MakeTestIpEndPoint());

            wrapped.Endpoint.ShouldBe(wrapped.Endpoint);
        }

        private static IPEndPoint MakeTestIpEndPoint()
        {
            return new IPEndPoint(new IPAddress(new byte[] {1, 2, 3, 4}), 8125);
        }
    }
}
