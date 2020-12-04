using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// A class representing an implementation of <see cref="IEndPointSource"/> that looks up
    /// the <see cref="EndPoint"/> for a DNS hostname to resolve its IP address.
    /// </summary>
    public class DnsLookupIpEndpointSource : IEndPointSource
    {
        private readonly string _hostName;
        private readonly int _port;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsLookupIpEndpointSource"/> class.
        /// </summary>
        /// <param name="hostName">The host name to look up the IP address for.</param>
        /// <param name="port">The port number to use for the end point.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="hostName"/> is <see langword="null"/>.
        /// </exception>
        public DnsLookupIpEndpointSource(string hostName, int port)
        {
            _hostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            _port = port;
        }

        /// <inheritdoc />
        public EndPoint GetEndpoint()
        {
            var address = GetIpAddressOfHost(_hostName);
            return new IPEndPoint(address, _port);
        }

        private static IPAddress GetIpAddressOfHost(string hostName)
        {
            var endpoints = Dns.GetHostAddresses(hostName);

            if (endpoints == null || endpoints.Length == 0)
            {
                throw new InvalidOperationException($"Failed to resolve any IP addresses for StatsD host '${hostName}' using DNS.");
            }

            IPAddress? result = null;

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
