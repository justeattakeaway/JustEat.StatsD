using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    public static class EndPointFactoryTests
    {
        [Fact]
        public static void CanParseIpValue()
        {
            var parsed = EndPointFactory.MakeEndPointSource("11.12.13.14", 8125, null);

            parsed.ShouldNotBeNull();

            var expected = new IPEndPoint(new IPAddress(new byte[] { 11, 12, 13, 14 }), 8125);
            parsed.GetEndpoint().ShouldBe(expected);
        }

        [Fact]
        public static void CanParseHostValue()
        {
            var parsed = EndPointFactory.MakeEndPointSource("somehost.somewhere.com", 8125, null);
            parsed.ShouldNotBeNull();
        }

        [Fact]
        public static void CanParseLocalhostValue()
        {
            var parsed = EndPointFactory.MakeEndPointSource("localhost", 8125, null);
            parsed.ShouldNotBeNull();
            parsed.GetEndpoint().ShouldNotBeNull();
        }

        [Fact]
        public static void CanParseCachedLocalhostValue()
        {
            var parsed = EndPointFactory.MakeEndPointSource("localhost", 8125, TimeSpan.FromMinutes(5));
            parsed.ShouldNotBeNull();
            parsed.GetEndpoint().ShouldNotBeNull();
        }

        [Fact]
        public static void CanParseHostValueWithCache()
        {
            var parsed = EndPointFactory.MakeEndPointSource("somehost.somewhere.com", 8125, TimeSpan.FromMinutes(5));
            parsed.ShouldNotBeNull();
        }

        [Fact]
        public static void CanParseLocalhostValueNoPort()
        {
            var endpoint = new IPEndPoint(127, 8125);
            var parsed = EndPointFactory.MakeEndPointSource(endpoint, null);
            parsed.ShouldNotBeNull();
            parsed.GetEndpoint().ShouldNotBeNull();
        }

        [Fact]
        public static void CanParseCachedLocalhostValueNoPort()
        {
            var endpoint = new IPEndPoint(127, 8125);
            var parsed = EndPointFactory.MakeEndPointSource(endpoint, TimeSpan.FromMinutes(5));
            parsed.ShouldNotBeNull();
            parsed.GetEndpoint().ShouldNotBeNull();
        }

        [Fact]
        public static void MakeEndPointSourceThrowsIfEndpointIsNull()
        {
            // Arrange
            EndPoint? endpoint = null;
            var endpointCacheDuration = TimeSpan.FromHours(1);

            // Act and Assert
            Assert.Throws<ArgumentNullException>("endpoint", () => EndPointFactory.MakeEndPointSource(endpoint!, endpointCacheDuration));
        }

        [Fact]
        public static void MakeEndPointSourceThrowsIfHostIsNull()
        {
            // Arrange
            string? host = null;
            int port = 8125;
            var endpointCacheDuration = TimeSpan.FromHours(1);

            // Act and Assert
            Assert.Throws<ArgumentException>("host", () => EndPointFactory.MakeEndPointSource(host!, port, endpointCacheDuration));
        }
    }
}
