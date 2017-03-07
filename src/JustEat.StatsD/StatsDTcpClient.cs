using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;

namespace JustEat.StatsD
{
    public class StatsDTcpClient : StatsDClient
    {
        public StatsDTcpClient(string hostNameOrAddress, int port) : base(hostNameOrAddress, port)
        {
        }

        public StatsDTcpClient(int endpointCacheDuration, string hostNameOrAddress, int port) : base(endpointCacheDuration, hostNameOrAddress, port)
        {
        }

        public override void CallClient(SocketAsyncEventArgs data)
        {
            using (var tcpClient = GetTcpClient())
            {
                tcpClient.Client.Connect(data.RemoteEndPoint);
                tcpClient.Client.SendPacketsAsync(data);
            }
        }

        private TcpClient GetTcpClient()
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient
                {
                    Client = { SendBufferSize = 0 }
                };
            }
            catch (SocketException e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Error Creating tcpClient :-  Message : {0}, Inner Exception {1}, StackTrace {2}.", e.Message, e.InnerException, e.StackTrace));
            }
            return client;
        }
    }
}