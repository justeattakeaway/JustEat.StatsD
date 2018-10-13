using System;
using System.Net;
using System.Net.Sockets;

namespace JustEat.StatsD
{
    public sealed class UdpListeners : IDisposable
    {
        private readonly UdpListener _listenerA;
        private readonly UdpListener _listenerB;

        public UdpListeners()
        {
            _listenerA = new UdpListener(EndpointA.Port);
            _listenerB = new UdpListener(EndpointB.Port);
        }

        public void Dispose()
        {
            _listenerA.Dispose();
            _listenerB.Dispose();
        }

        public static IPEndPoint EndpointA { get; } = new IPEndPoint(IPAddress.Loopback, 7125);

        public static IPEndPoint EndpointB { get; } = new IPEndPoint(IPAddress.Loopback, 7126);

        private sealed class UdpListener : IDisposable
        {
            private readonly Socket _socket;

            public UdpListener(int port)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var endPoint = new IPEndPoint(IPAddress.Loopback, port);
                _socket.Bind(endPoint);
            }

            public void Dispose()
            {
                _socket.Dispose();
            }
        }
    }
}
