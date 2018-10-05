using System;
using System.Net;
using JustEat.StatsD.EndpointLookups;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public class SocketTransportTests
    {
        [Fact]
        public static void ValidSocketTransportCanBeConstructed()
        {
            var transport = new SocketTransport(LocalStatsEndpoint(), SocketProtocol.Udp);

            transport.ShouldNotBeNull();
        }

        [Fact]
        public static void SocketTransportCanSendOverUdpWithoutError()
        {
            var transport = new SocketTransport(LocalStatsEndpoint(), SocketProtocol.Udp);

            transport.Send("testStat");
        }

        [Fact]
        public static void SocketTransportCanSendOverIPWithoutError()
        {
            var transport = new SocketTransport(LocalStatsEndpoint(), SocketProtocol.IP);

            transport.Send("testStat");
        }

        [Fact]
        public static void NullEndpointSourceThrowsInConstructor()
        {
            Should.Throw<ArgumentNullException>(
                () => new SocketTransport(null, SocketProtocol.IP));

        }

        [Fact]
        public static void InvalidSocketProtocolThrowsInConstructor()
        {
            Should.Throw<ArgumentOutOfRangeException>(
                () => new SocketTransport(LocalStatsEndpoint(), (SocketProtocol)42));
        }

        private static IPEndPointSource LocalStatsEndpoint()
        {
            return new SimpleIpEndpoint(new IPEndPoint(IPAddress.Loopback, StatsDConfiguration.DefaultPort));
        }
    }
}
