using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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

    }
}
