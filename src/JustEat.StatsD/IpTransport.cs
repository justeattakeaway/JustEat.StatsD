using System;
using System.Collections.Generic;
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

        public void Send(string metric)
        {
            Send(new[] {metric});
        }

        public void Send(IEnumerable<string> metrics)
        {
            var endpoint = _endpointSource.GetEndpoint();
            var packets = metrics.ToMaximumBytePackets();

            using (var socket = MakeIpDatagramSocket())
            {
                foreach (var packet in packets)
                {
                    socket.SendTo(packet, endpoint);
                }
            }
        }

        private static Socket MakeIpDatagramSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
