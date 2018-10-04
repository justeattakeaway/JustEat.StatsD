using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace JustEat.StatsD
{
    internal static class SocketFactory
    {
        internal static Socket For(SocketProtocol socketProtocol)
        {
            switch (socketProtocol)
            {
                case SocketProtocol.Ip:
                    return ForIp();

                case SocketProtocol.Udp:
                    return ForUdp();

                default:
                    throw new InvalidOperationException($"Unknown socketProtocol {socketProtocol}"); 
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
