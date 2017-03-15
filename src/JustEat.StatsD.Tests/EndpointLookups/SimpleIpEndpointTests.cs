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
            var ipEndpoint = new IPEndPoint(new IPAddress(new byte[] { 1, 2, 3, 4 }), 8125);

            var mapped = new SimpleIpEndpoint(ipEndpoint);

            var expected = new IPEndPoint(new IPAddress(new byte[] {1, 2, 3, 4}), 8125);
            mapped.Endpoint.ShouldBe(expected);
        }
    }
}
