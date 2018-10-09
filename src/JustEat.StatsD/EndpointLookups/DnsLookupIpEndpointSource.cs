using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// lookup IPAddress using DNS to find the host's IP
    /// </summary>
    public class DnsLookupIpEndpointSource : IPEndPointSource
    {
        private readonly string _hostName;
        private readonly int _port;

        public DnsLookupIpEndpointSource(string hostName, int port)
        {
            _hostName = hostName;
            _port = port;
        }

        public IPEndPoint GetEndpoint()
        {
            return new IPEndPoint(GetIpAddressOfHost(_hostName), _port);
        }

        private static IPAddress GetIpAddressOfHost(string hostName)
        {
            var endpoints = Dns.GetHostAddresses(hostName);

            if (endpoints == null || endpoints.Length == 0)
            {
                throw new Exception($"DNS did not find any addresses for statsd host '${hostName}'");
            }

            IPAddress result = null;

            if (endpoints.Length > 1)
            {
                result = endpoints.FirstOrDefault(p => p.AddressFamily == AddressFamily.InterNetwork);
            }

            if (result == null)
            {
                result = endpoints[0];
            }

            return result;
        }
    }
}
