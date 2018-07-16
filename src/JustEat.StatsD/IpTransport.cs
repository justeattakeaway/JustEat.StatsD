using System;
using System.Net.Sockets;
using System.Text;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    public class IpTransport : IStatsDTransport
    {
        private readonly IPEndPointSource _endpointSource;

        private readonly Socket _socket = CreateSocket();

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

            _socket.SendTo(bytes, endpoint);
        }

        private static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
