using System;
using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    public static class EndpointParser
    {
        public static IPEndPointSource MakeEndPointSource(string host, int port, TimeSpan? endpointCacheDuration)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("statsd host is null or empty");
            }

            IPAddress address;
            if (IPAddress.TryParse(host, out address))
            {
                //if we were given an IP instead of a hostname, 
                // we can happily keep it the life of this class
                var endpoint = new IPEndPoint(address, port);
                return new SimpleIpEndpoint(endpoint);
            }

            // we have a host name,
            // so we use DNS lookup
            var uncachedLookup = new DnsLookupIpEndpointSource(host, port);
            if (!endpointCacheDuration.HasValue)
            {
                return uncachedLookup;
            }

            return new CachedIpEndpointSource(uncachedLookup, endpointCacheDuration.Value);
        }
    }
}