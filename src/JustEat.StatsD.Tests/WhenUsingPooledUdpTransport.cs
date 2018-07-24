using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using JustEat.StatsD.EndpointLookups;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace JustEat.StatsD
{
    public sealed class UdpListener : IDisposable
    {
        private readonly Socket _socket;
        private EndPoint _endPoint;

        public class State
        {
            public const int BufSize = 8 * 1024;
            public readonly byte[] Buffer = new byte[BufSize];
        }

        public UdpListener(int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _endPoint = new IPEndPoint(IPAddress.Loopback, port);
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        }

        public void Start()
        {
            var state = new State();
            AsyncCallback recv = null;
            
            _socket.Bind(_endPoint);
            _socket.BeginReceiveFrom(state.Buffer, 0, State.BufSize, SocketFlags.None, ref _endPoint, recv = (ar) =>
            {
                var so = (State)ar.AsyncState;
                int _ = _socket.EndReceiveFrom(ar, ref _endPoint);
                _socket.BeginReceiveFrom(so.Buffer, 0, State.BufSize, SocketFlags.None, ref _endPoint, recv, so);
            }, state);
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }

    public sealed class Servers : IDisposable
    {
        private readonly UdpListener _one;
        private readonly UdpListener _two;

        public Servers()
        {
            _one = new UdpListener(8125);
            _two = new UdpListener(8126);

            Start();
        }

        public void Start()
        {
            _one.Start();
            _two.Start();
        }

        public void Dispose()
        {
            _one.Dispose();
            _two.Dispose();
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

        public WhenUsingPooledUdpTransport(Servers servers)
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
                    target.Send("mycustommetric");
                });
            }
        }

        [Fact]
        public void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownParallel_WithChangingEndpoints()
        {
            // Arrange
            var endPointSource1 = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);
            var endPointSource2 = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8126, null);

            var endPointSource = new MilisecondSwitcher(endPointSource1, endPointSource2);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                Parallel.For(0, 10_000, _ =>
                {
                    target.Send("mycustommetric");
                });
            }
        }

    }
}
