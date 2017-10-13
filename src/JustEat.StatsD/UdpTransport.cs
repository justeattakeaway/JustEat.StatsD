using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    public class UdpTransport : IStatsDTransport
    {
        private static readonly SimpleObjectPool<SocketAsyncEventArgs> EventArgsPool
            = new SimpleObjectPool<SocketAsyncEventArgs>(30, pool => new PoolAwareSocketAsyncEventArgs(pool));

        private readonly IPEndPointSource _endpointSource;

        public UdpTransport(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        public bool Send(string metric)
        {
            return Send(new[] {metric});
        }

        public bool Send(IEnumerable<string> metrics)
        {
            var data = EventArgsPool.Pop();
            //firehose alert! -- keep it moving!
            if (data == null)
            {
                return false;
            }

            try
            {
                data.RemoteEndPoint = _endpointSource.GetEndpoint();
                data.SendPacketsElements = metrics.ToMaximumBytePackets()
                    .Select(bytes => new SendPacketsElement(bytes, 0, bytes.Length, true))
                    .ToArray();

                using (var udpClient = GetUdpClient())
                {
                    udpClient.Client.Connect(data.RemoteEndPoint);
                    udpClient.Client.SendPacketsAsync(data);
                }

                return true;
            }
            //fire and forget, so just eat intermittent failures / exceptions
            catch (Exception)
            {
            }

            return false;
        }

        public UdpClient GetUdpClient()
        {
            UdpClient client = null;
            try
            {
                client = new UdpClient
                {
                    Client = { SendBufferSize = 0 }
                };
            }
            catch (SocketException)
            {
            }
            return client;
        }
    }
}
