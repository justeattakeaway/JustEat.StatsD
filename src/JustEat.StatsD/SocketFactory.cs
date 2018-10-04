using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace JustEat.StatsD
{
    public enum SocketTransport
    {
        Udp,
        Ip
    }

    internal static class SocketFactory
    {
        internal static Socket For(SocketTransport transport)
        {
            switch (transport)
            {
                case SocketTransport.Ip:
                    return ForIp();

                case SocketTransport.Udp:
                    return ForUdp();

                default:
                    throw new InvalidOperationException($"Unknown transport {transport}"); 
            }
        }

        internal static Socket ForUdp()
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

        internal static Socket ForIp()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
