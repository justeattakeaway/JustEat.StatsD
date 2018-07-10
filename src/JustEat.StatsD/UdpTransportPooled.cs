using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    public class UdpTransportPooled : IStatsDTransport
    {
        private readonly IPEndPointSource _endpointSource;

        [ThreadStatic] private static Socket _socket;
        [ThreadStatic] private static IPEndPoint _ipEndPoint;

        public UdpTransportPooled(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        public void Send(ReadOnlySpan<byte> metric)
        {
            if (metric.Length == 0)
                return;

            var socket = GetSocket(_endpointSource.GetEndpoint());
            socket.Send(metric);
        }

        private static Socket GetSocket(IPEndPoint endPoint)
        {
            if (_socket == null || !ReferenceEquals(_ipEndPoint, endPoint))
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

#if !NET451
                // See https://github.com/dotnet/corefx/pull/17853#issuecomment-291371266
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) socket.SendBufferSize = 0;
#else
                socket.SendBufferSize = 0;
#endif

                _socket?.Dispose();
                _ipEndPoint = endPoint;
                socket.Connect(endPoint);
                _socket = socket;
                return socket;
            }

            return _socket;
        }
    }
}
