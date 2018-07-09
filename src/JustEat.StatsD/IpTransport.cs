using System;
using System.Buffers;
using System.Net.Sockets;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    public class IpTransport : IStatsDTransport
    {
        private readonly IPEndPointSource _endpointSource;

        public IpTransport(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        public void Send(ReadOnlySpan<byte> metric)
        {
            if (metric.Length == 0)
                return;

            var endpoint = _endpointSource.GetEndpoint();
            using (var socket = CreateSocket())
            {
                socket.Connect(endpoint);
                socket.Send(metric);
            }
        }

        private static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
