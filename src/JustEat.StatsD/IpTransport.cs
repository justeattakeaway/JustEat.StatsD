using System;
using System.Net.Sockets;
using System.Text;
using JustEat.StatsD.EndpointLookups;
using JustEat.StatsD.V2;

namespace JustEat.StatsD
{
    public class IpTransport : IStatsDTransport, IStatsDTransportV2
    {
        private readonly IPEndPointSource _endpointSource;

        public IpTransport(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        public void Send(string metric)
        {
            if (string.IsNullOrWhiteSpace(metric))
            {
                return;
            }

            var endpoint = _endpointSource.GetEndpoint();
            var bytes = Encoding.UTF8.GetBytes(metric);

            using (var socket = CreateSocket())
            {
                socket.SendTo(bytes, endpoint);
            }
        }

        private static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }

        public void Send(ArraySegment<byte> metric)
        {
            if (metric.Count == 0)
            {
                return;
            }

            var endpoint = _endpointSource.GetEndpoint();
            using (var socket = CreateSocket())
            {
                socket.SendTo(metric.Array, 0, metric.Count, SocketFlags.None, endpoint);
            }
        }
    }
}
