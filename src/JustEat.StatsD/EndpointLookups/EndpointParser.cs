using System;
using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    public static class EndpointParser
    {
        public static IPEndPointSource MakeEndPointSource(IPEndPoint endpoint, TimeSpan? endpointCacheDuration)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            IPEndPointSource source = new SimpleIpEndpoint(endpoint);

            if (!endpointCacheDuration.HasValue)
            {
                return source;
            }

            return new CachedIpEndpointSource(source, endpointCacheDuration.Value);
        }

        public static IPEndPointSource MakeEndPointSource(string host, int port, TimeSpan? endpointCacheDuration)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("statsd host is null or empty");
            }

            if (IPAddress.TryParse(host, out IPAddress address))
            {
                // If we were given an IP instead of a hostname, 
                // we can happily keep it the life of this class
                var endpoint = new IPEndPoint(address, port);
                return new SimpleIpEndpoint(endpoint);
            }

            // We have a host name, so we use DNS lookup
            var uncachedLookup = new DnsLookupIpEndpointSource(host, port);

            if (!endpointCacheDuration.HasValue)
            {
                return uncachedLookup;
            }

            return new CachedIpEndpointSource(uncachedLookup, endpointCacheDuration.Value);
        }
    }
}
