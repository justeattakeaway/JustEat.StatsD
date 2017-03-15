using System.Net;
using Shouldly;
using Xunit;

namespace JustEat.StatsD.EndpointLookups
{
    public static class EndpointParserTests
    {
        [Fact]
        public static void CanParseIpValue()
        {
            var parsed = EndpointParser.MakeEndPointSource("11.12.13.14", 8125, null);

            parsed.ShouldNotBeNull();

            var expected = new IPEndPoint(new IPAddress(new byte[] { 11, 12, 13, 14 }), 8125);
            parsed.Endpoint.ShouldBe(expected);
        }

        [Fact]
        public static void CanParseHostValue()
        {
            var parsed = EndpointParser.MakeEndPointSource("somehost.somewhere.com", 8125, null);
            parsed.ShouldNotBeNull();
        }

        [Fact]
        public static void CanParseLocalhostValue()
        {
            var parsed = EndpointParser.MakeEndPointSource("localhost", 8125, null);
            parsed.ShouldNotBeNull();
            parsed.Endpoint.ShouldNotBeNull();
        }

        [Fact]
        public static void CanParseCachedLocalhostValue()
        {
            var parsed = EndpointParser.MakeEndPointSource("localhost", 8125, 1234);
            parsed.ShouldNotBeNull();
            parsed.Endpoint.ShouldNotBeNull();
        }

        [Fact]
        public static void CanParseHostValueWithCache()
        {
            var parsed = EndpointParser.MakeEndPointSource("somehost.somewhere.com", 8125, 1234);
            parsed.ShouldNotBeNull();
        }
    }
}
