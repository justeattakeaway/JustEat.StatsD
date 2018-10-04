using System;
using System.Net.Sockets;
using System.Text;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    public class IpTransport : IStatsDTransport, IStatsDBufferedTransport
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

            using (var socket = Transport.IpSocket())
            {
                socket.SendTo(bytes, endpoint);
            }
        }

        public void Send(in ArraySegment<byte> metric)
        {
            if (metric.Array == null || metric.Count == 0)
            {
                return;
            }

            var endpoint = _endpointSource.GetEndpoint();
            using (var socket = Transport.IpSocket())
            {
                socket.SendTo(metric.Array, 0, metric.Count, SocketFlags.None, endpoint);
            }
        }
    }
}
