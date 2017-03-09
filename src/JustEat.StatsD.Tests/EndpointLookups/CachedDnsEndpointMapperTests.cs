using System;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;

namespace JustEat.StatsD.EndpointLookups
{
    public static class CachedDnsEndpointMapperTests
    {
        [Fact]
        public static async Task IPEndpointsAreCached()
        {
            // Arrange
            int port = 1234;
            string hostName = "myhost.name.com";
            int secondsCacheDuration = 2;

            var mock = new Mock<IDnsEndpointMapper>();

            mock.Setup((p) => p.GetIPEndPoint(hostName, port))
                .Returns(() => new System.Net.IPEndPoint(123456789, 4567));

            CachedDnsEndpointMapper target = new CachedDnsEndpointMapper(mock.Object, secondsCacheDuration);

            // Act
            var ip1 = target.GetIPEndPoint(hostName, port);
            var ip2 = target.GetIPEndPoint(hostName, port);

            // Assert
            mock.Verify((p) => p.GetIPEndPoint(hostName, port), Times.Once());
            ip1.ShouldBeSameAs(ip2);

            // Arrange
            await Task.Delay(TimeSpan.FromSeconds(secondsCacheDuration));

            // Act
            var ip3 = target.GetIPEndPoint(hostName, port);
            var ip4 = target.GetIPEndPoint(hostName, port);

            // Assert
            mock.Verify((p) => p.GetIPEndPoint(hostName, port), Times.Exactly(2));
            ip1.ShouldNotBeSameAs(ip3);
            ip3.ShouldBeSameAs(ip4);
        }
    }
}
