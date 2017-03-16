using System.Net;

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
            var endpoints = Dns.GetHostAddressesAsync(hostName)
                .GetAwaiter().GetResult();
            return endpoints[0];
        }
    }
}