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
            Socket socket = null;
            byte[] buffer = null;
            
            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(metric.Length);
                metric.CopyTo(buffer);
                socket = CreateSocket();
                socket.SendTo(buffer, metric.Length, SocketFlags.None, endpoint);
            }
            finally 
            {
                if (buffer!= null) ArrayPool<byte>.Shared.Return(buffer);
                socket?.Dispose();
            }
        }

        private static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
