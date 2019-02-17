using System;
using System.Net;
using JustEat.StatsD.EndpointLookups;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class SocketTransportTests
    {
        [Fact]
        public static void ValidSocketTransportCanBeConstructed()
        {
            using (var transport = new SocketTransport(LocalStatsEndpoint(), SocketProtocol.Udp))
            {
                transport.ShouldNotBeNull();
            }
        }

        [Fact]
        public static void SocketTransportCanSendOverUdpWithoutError()
        {
            using (var transport = new SocketTransport(LocalStatsEndpoint(), SocketProtocol.Udp))
            {
                transport.Send("teststat:1|c");
            }
        }

        [Fact]
        public static void SocketTransportCanSendOverIPWithoutError()
        {
            using (var transport = new SocketTransport(LocalStatsEndpoint(), SocketProtocol.IP))
            {
                transport.Send("teststat:1|c");
            }
        }

        [Fact]
        public static void NullEndpointSourceThrowsInConstructor()
        {
            Assert.Throws<ArgumentNullException>(
                "endPointSource",
                () => new SocketTransport(null, SocketProtocol.IP));
        }

        [Fact]
        public static void InvalidSocketProtocolThrowsInConstructor()
        {
            var socketProtocol = (SocketProtocol)42;

            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                "socketProtocol",
                () => new SocketTransport(LocalStatsEndpoint(), socketProtocol));

            exception.ActualValue.ShouldBe(socketProtocol);
        }

        [Fact]
        public static void SocketTransportIsNoopForNullArray()
        {
            using (var transport = new SocketTransport(LocalStatsEndpoint(), SocketProtocol.IP))
            {
                transport.Send(new ArraySegment<byte>());
            }
        }

        [Fact]
        public static void SocketTransportIsNoopForEmptyArray()
        {
            using (var transport = new SocketTransport(LocalStatsEndpoint(), SocketProtocol.IP))
            {
                transport.Send(new ArraySegment<byte>(Array.Empty<byte>()));
            }
        }

        private static IEndPointSource LocalStatsEndpoint()
        {
            return new SimpleEndpointSource(new IPEndPoint(IPAddress.Loopback, StatsDConfiguration.DefaultPort));
        }
    }
}
