using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using JustEat.StatsD.EndpointLookups;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace JustEat.StatsD
{
    internal sealed class Servers : IDisposable
    {
        private readonly UdpListener _one;
        private readonly UdpListener _two;

        public Servers()
        {
            _one = new UdpListener(8125);
            _two = new UdpListener(8126);
        }

        public void Dispose()
        {
            _one.Dispose();
            _two.Dispose();
        }

        private sealed class UdpListener : IDisposable
        {
            private readonly Socket _socket;

            public UdpListener(int port)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var endPoint = new IPEndPoint(IPAddress.Loopback, port);
                _socket.Bind(endPoint);            
            }      

            public void Dispose()
            {
                _socket.Dispose();
            }
        }
    }

    [CollectionDefinition("REAL_UDP", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<Servers>
    {
    }

    [Collection("REAL_UDP")]
    public class WhenUsingPooledUdpTransport 
    {
        private class MilisecondSwitcher : IPEndPointSource
        {
            private readonly IPEndPointSource _endpointSource1;
            private readonly IPEndPointSource _endpointSource2;

            public MilisecondSwitcher(IPEndPointSource endpointSource1, IPEndPointSource endpointSource2)
            {
                _endpointSource1 = endpointSource1;
                _endpointSource2 = endpointSource2;
            }

            public IPEndPoint GetEndpoint()
            {
                return DateTime.Now.Millisecond % 2 == 0 ?
                    _endpointSource1.GetEndpoint() :
                    _endpointSource2.GetEndpoint();
            }
        }

        internal WhenUsingPooledUdpTransport(Servers servers)
        {
        }

        [Fact]
        public void AMetricCanBeSentWithoutAnExceptionBeingThrown()
        {
            // Arrange
            var endPointSource = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                // Act and Assert
                target.Send("mycustommetric");
            }
        }

        [Fact]
        public void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownSerial()
        {
            // Arrange
            var endPointSource = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                for (int i = 0; i < 10_000; i++)
                {
                    // Act and Assert
                    target.Send("mycustommetric");
                }
            }
        }

        [Fact]
        public void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownParallel()
        {
            // Arrange
            var endPointSource = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                Parallel.For(0, 10_000, _ =>
                {
                    // Act and Assert
                    target.Send("mycustommetric");
                });
            }
        }

        [Fact]
        public static void EndpointSwitchShouldNotCauseExceptionsSequential()
        {
            // Arrange
            var endPointSource1 = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);
            var endPointSource2 = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8126, null);
            
            using (var target = new PooledUdpTransport(new MilisecondSwitcher(endPointSource2, endPointSource1)))
            {
                for (int i = 0; i < 10_000; i++)
                {
                    // Act and Assert
                    target.Send("mycustommetric");
                }
            }
        }

        [Fact]
        public static void EndpointSwitchShouldNotCauseExceptionsParallel()
        {
            // Arrange
            var endPointSource1 = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);
            var endPointSource2 = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8126, null);
            
            using (var target = new PooledUdpTransport(new MilisecondSwitcher(endPointSource2, endPointSource1)))
            {
                Parallel.For(0, 10_000, _ =>
                {
                    // Act and Assert
                    target.Send("mycustommetric");
                });
            }
        }
    }
}
