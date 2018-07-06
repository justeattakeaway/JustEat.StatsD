using System;
using System.Buffers;
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
            var rent = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(metric.Length));
            var bytes = Encoding.UTF8.GetBytes(metric, rent);

            using (var socket = CreateSocket())
            {
                socket.SendTo(rent, bytes, SocketFlags.None, endpoint);
            }

            ArrayPool<byte>.Shared.Return(rent);
        }

        private static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
