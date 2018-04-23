using System;
using System.Net.Sockets;
using System.Text;
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

        public void Send(string metric)
        {
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
