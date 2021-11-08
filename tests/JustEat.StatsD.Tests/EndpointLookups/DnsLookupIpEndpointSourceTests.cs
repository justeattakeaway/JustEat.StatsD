using System.Net;
using System.Net.Sockets;

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
            EndPoint actual = target.GetEndpoint();

            // Assert
            actual.ShouldNotBeNull();
            actual.AddressFamily.ShouldBe(AddressFamily.InterNetwork);

            var ipActual = actual as IPEndPoint;

            ipActual.ShouldNotBeNull();
            ipActual!.Address.ShouldBe(IPAddress.Parse("127.0.0.1"));
            ipActual.Port.ShouldBe(8125);
        }

        [Fact]
        public static void ConstructorThrowsIfHostNameIsNull()
        {
            // Arrange
            string? hostName = null;
            int port = 123;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("hostName", () => new DnsLookupIpEndpointSource(hostName!, port));
        }
    }
}
