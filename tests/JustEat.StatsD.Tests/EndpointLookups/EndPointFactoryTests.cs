using System;
using System.Net;
using Shouldly;
using Xunit;

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
    }
}
