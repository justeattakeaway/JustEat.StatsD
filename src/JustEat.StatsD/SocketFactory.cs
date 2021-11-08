using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace JustEat.StatsD
{
    internal static class SocketFactory
    {
        internal static Socket For(SocketProtocol socketProtocol)
        {
            return socketProtocol switch
            {
                SocketProtocol.IP => ForIp(),

                SocketProtocol.Udp => ForUdp(),

                _ => throw new InvalidOperationException($"Unknown {nameof(SocketProtocol)} value {socketProtocol} specified."),
            };
        }

        internal static Socket ForUdp()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // See https://github.com/dotnet/corefx/pull/17853#issuecomment-291371266
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                socket.SendBufferSize = 0;
            }

            return socket;
        }

        internal static Socket ForIp()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
