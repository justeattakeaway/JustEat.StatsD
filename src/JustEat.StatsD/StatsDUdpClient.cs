using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;

namespace JustEat.StatsD
{
    public class StatsDUdpClient : StatsDClient, IStatsDUdpClient
    {
        public StatsDUdpClient(string hostNameOrAddress, int port) : base(hostNameOrAddress, port)
        {
        }

        public StatsDUdpClient(int endpointCacheDuration, string hostNameOrAddress, int port) : base(endpointCacheDuration, hostNameOrAddress, port)
        {
        }

        public override void CallClient(SocketAsyncEventArgs data)
        {
            using (var udpClient = GetUdpClient())
            {
                udpClient.Client.Connect(data.RemoteEndPoint);
                udpClient.Client.SendPacketsAsync(data);
            }
        }

        private UdpClient GetUdpClient()
        {
            UdpClient client = null;
            try
            {
                client = new UdpClient()
                {
                    Client = { SendBufferSize = 0 }
                };
            }
            catch (SocketException e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Error Creating udpClient :-  Message : {0}, Inner Exception {1}, StackTrace {2}.", e.Message, e.InnerException, e.StackTrace));
            }
            return client;
        }
    }
}
