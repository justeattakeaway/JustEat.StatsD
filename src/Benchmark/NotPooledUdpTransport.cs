using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    public class NotPooledUdpTransport : IStatsDTransport
    {
        private readonly IPEndPointSource _endpointSource;

        public NotPooledUdpTransport(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        public void Send(string metric)
        {
            if (string.IsNullOrWhiteSpace(metric))
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(metric);
            var endpoint = _endpointSource.GetEndpoint();

            using (var socket = CreateSocket())
            {
                socket.SendTo(bytes, endpoint);
            }
        }

        private static Socket CreateSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // See https://github.com/dotnet/corefx/pull/17853#issuecomment-291371266
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                socket.SendBufferSize = 0;
            }

            return socket;
        }
    }
}
