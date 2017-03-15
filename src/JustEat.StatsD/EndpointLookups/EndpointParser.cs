using System;
using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    public static class EndpointParser
    {
        public static IPEndPointSource MakeEndPointSource(string hostName, int port, int? endpointCacheDuration)
        {
            if (string.IsNullOrWhiteSpace(hostName))
            {
                throw new ArgumentException(nameof(hostName));
            }


            IPAddress address;
            if (IPAddress.TryParse(hostName, out address))
            {
                //if we were given an IP instead of a hostname, 
                // we can happily keep it the life of this class
                var endpoint = new IPEndPoint(address, port);
                return new SimpleIpEndpoint(endpoint);
            }

            // we have a host name,
            // so we use DNS lookup
            var uncachedLookup = new DnsLookupIpEndpointSource(hostName, port);
            if (!endpointCacheDuration.HasValue)
            {
                return uncachedLookup;
            }

            return new CachedIpEndpointSource(uncachedLookup, endpointCacheDuration.Value);
        }
    }
}