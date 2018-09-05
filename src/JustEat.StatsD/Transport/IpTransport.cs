using System;
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

        public void Send(in Data metric)
        {
            var endpoint = _endpointSource.GetEndpoint();

            using (var socket = CreateSocket())
            {
#if NETCOREAPP2_1
                socket.Connect(endpoint);
                socket.Send(metric.GetSpan());
#else
                socket.SendTo(metric.GetArray(), endpoint);
#endif
            }
        }

        private static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
