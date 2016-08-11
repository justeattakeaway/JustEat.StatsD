using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    ///     Base endpoint provider.
    ///     Basically an adapter around IpEndpoint creation.
    /// </summary>
    public class DnsEndpointProvider : IDnsEndpointMapper
    {
        public IPEndPoint GetIPEndPoint(string hostName, int port)
        {
            var endpoints = Dns.GetHostAddressesAsync(hostName).Result;
            return new IPEndPoint(endpoints[0], port);
        }
    }
}
