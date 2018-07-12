using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
