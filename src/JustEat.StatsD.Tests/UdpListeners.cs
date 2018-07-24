using System;
using System.Net;
using System.Net.Sockets;

namespace JustEat.StatsD
{
    public sealed class UdpListeners : IDisposable
    {
        private readonly UdpListener _one;
        private readonly UdpListener _two;

        public UdpListeners()
        {
            _one = new UdpListener(8125);
            _two = new UdpListener(8126);
        }

        public void Dispose()
        {
            _one.Dispose();
            _two.Dispose();
        }

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