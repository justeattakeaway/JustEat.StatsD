using System;
using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    //// TODO Is this more of a EndPointFactory?

    /// <summary>
    /// A class containing methods to instances of <see cref="IEndPointSource"/>. This class cannot be inherited.
    /// </summary>
    public static class EndpointParser
    {
        /// <summary>
        /// Creates an <see cref="IEndPointSource"/> from the specified <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="endpoint">The <see cref="EndPoint"/> to create the end point source for.</param>
        /// <param name="endpointCacheDuration">The optional period of time to cache the end point value for.</param>
        /// <returns>
        /// The created instance of <see cref="IEndPointSource"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="endpoint"/> is <see langword="null"/>.
        /// </exception>
        public static IEndPointSource MakeEndPointSource(EndPoint endpoint, TimeSpan? endpointCacheDuration)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            IEndPointSource source = new SimpleEndpointSource(endpoint);

            if (endpointCacheDuration.HasValue)
            {
                source = new CachedEndpointSource(source, endpointCacheDuration.Value);
            }

            return source;
        }

        /// <summary>
        /// Creates an <see cref="IEndPointSource"/> from the specified host IP address or name and port.
        /// </summary>
        /// <param name="host">The host name of IP address of the statsd server.</param>
        /// <param name="port">The port number to use for the end point.</param>
        /// <param name="endpointCacheDuration">The optional period of time to cache the end point value for.</param>
        /// <returns>
        /// The created instance of <see cref="IEndPointSource"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="host"/> is either <see langword="null"/> or the empty string.
        /// </exception>
        public static IEndPointSource MakeEndPointSource(string host, int port, TimeSpan? endpointCacheDuration)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("The statsd host IP address or name is null or empty.", nameof(host));
            }

            IEndPointSource source;

            if (IPAddress.TryParse(host, out IPAddress address))
            {
                // If we were given an IP instead of a hostname, 
                // we can happily keep it the life of this class
                var value = new IPEndPoint(address, port);
                source = new SimpleEndpointSource(value);
            }
            else
            {
                // We have a host name, so we use DNS lookup
                source = new DnsLookupIpEndpointSource(host, port);

                if (endpointCacheDuration.HasValue)
                {
                    source = new CachedEndpointSource(source, endpointCacheDuration.Value);
                }
            }

            return source;
        }
    }
}
