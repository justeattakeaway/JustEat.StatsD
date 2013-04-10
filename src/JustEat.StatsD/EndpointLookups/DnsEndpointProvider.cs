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
            return new IPEndPoint(Dns.GetHostAddresses(hostName)[0], port);
        }
    }
}
