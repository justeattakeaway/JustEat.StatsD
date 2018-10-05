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
            var ipEndpoint = new SimpleIpEndpoint(new IPEndPoint(IPAddress.Loopback, StatsDConfiguration.DefaultPort));

            var instance = new SocketTransport(ipEndpoint, SocketProtocol.Udp);

            instance.ShouldNotBeNull();
        }

        [Fact]
        public static void ValidSocketTransportCanSendWithoutError()
        {
            var ipEndpoint = new SimpleIpEndpoint(new IPEndPoint(IPAddress.Loopback, StatsDConfiguration.DefaultPort));

            var instance = new SocketTransport(ipEndpoint, SocketProtocol.Udp);

            instance.ShouldNotBeNull();
            instance.Send("testStat");
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
            var ipEndpoint = new SimpleIpEndpoint(new IPEndPoint(IPAddress.Loopback, 123));

            Should.Throw<ArgumentOutOfRangeException>(
                () => new SocketTransport(ipEndpoint, (SocketProtocol)42));
        }
    }
}
