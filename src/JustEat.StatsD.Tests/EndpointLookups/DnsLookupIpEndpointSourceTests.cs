using System.Net;
using System.Net.Sockets;
using Shouldly;
using Xunit;

namespace JustEat.StatsD.EndpointLookups
{
    public static class DnsLookupIpEndpointSourceTests
    {
        [Fact]
        public static void GetEndpointPrefersIPV4WhenHostnameIsLocalhost()
        {
            // Arrange
            var target = new DnsLookupIpEndpointSource("localhost", 8125);

            // Act
            IPEndPoint actual = target.GetEndpoint();

            // Assert
            actual.ShouldNotBeNull();
            actual.AddressFamily.ShouldBe(AddressFamily.InterNetwork);
            actual.Address.ShouldBe(IPAddress.Parse("127.0.0.1"));
            actual.Port.ShouldBe(8125);
        }
    }
}
