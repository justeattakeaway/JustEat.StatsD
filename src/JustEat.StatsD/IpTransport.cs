using System;
using System.Net.Sockets;
using System.Text;
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

        public void Send(string metric)
        {
            var endpoint = _endpointSource.GetEndpoint();
            var bytes = Encoding.UTF8.GetBytes(metric);

            using (var socket = MakeIpDatagramSocket())
            {
                socket.Connect(endpoint);
                socket.Send(bytes);
            }
        }

        private static Socket MakeIpDatagramSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
