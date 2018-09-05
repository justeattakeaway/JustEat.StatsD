using System;
using System.Net.Sockets;
using JustEat.StatsD.EndpointLookups;

#if !NET451
using System.Runtime.InteropServices;
#endif

namespace JustEat.StatsD.V2
{
    public sealed class UdpTransportV2 : IStatsDTransportV2
    {
        private readonly IPEndPointSource _endpointSource;

        public UdpTransportV2(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        public void Send(ArraySegment<byte> metric)
        {
            if (metric.Array == null)
                return;

            var endpoint = _endpointSource.GetEndpoint();

            using (var socket = CreateSocket())
            {
                socket.SendTo(metric.Array, metric.Offset, metric.Count, SocketFlags.None, endpoint);
            }
        }

        internal static Socket CreateSocket()
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
