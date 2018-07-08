using System;
using System.Buffers;
using System.Net.Sockets;
#if !NET451
using System.Runtime.InteropServices;
#endif
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    public class UdpTransport : IStatsDTransport
    {
        private readonly IPEndPointSource _endpointSource;

        public UdpTransport(IPEndPointSource endPointSource)
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
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

#if !NET451
            // See https://github.com/dotnet/corefx/pull/17853#issuecomment-291371266
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                socket.SendBufferSize = 0;
            }
#else
            socket.SendBufferSize = 0;
#endif

            return socket;
        }
    }
}
