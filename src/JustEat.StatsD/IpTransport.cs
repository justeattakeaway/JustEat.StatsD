using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    public class IpTransport : IStatsDTransport
    {
        private readonly IPEndPointSource _endpointSource;

        public IpTransport(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        public bool Send(string metric)
        {
            return Send(new[] {metric});
        }

        public bool Send(IEnumerable<string> metrics)
        {
            try
            {
                var endpoint = _endpointSource.GetEndpoint();
                var packets = metrics.ToMaximumBytePackets();

                using (var socket = MakeIpDatagramSocket())
                {
                    foreach (var packet in packets)
                    {
                        socket.SendTo(packet, endpoint);
                    }
                }

#if DEBUG
                Trace.TraceInformation("statsd: {0}", string.Join(",", metrics));
#endif

                return true;
            }
            //fire and forget, so just eat intermittent failures / exceptions
            catch (Exception e)
            {
                Trace.TraceError("General Exception when sending metric data to statsD :- Message : {0}, Inner Exception {1}, StackTrace {2}.", e.Message, e.InnerException, e.StackTrace);
            }

            return false;
        }

        private static Socket MakeIpDatagramSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
        }
    }
}
