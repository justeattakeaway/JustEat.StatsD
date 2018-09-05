using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class representing an implementation of <see cref="IStatsDTransport"/> that uses UDP.
    /// </summary>
    public class UdpTransport : IStatsDTransport
    {
        private readonly IPEndPointSource _endpointSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpTransport"/> class.
        /// </summary>
        /// <param name="endPointSource">The <see cref="IPEndPointSource"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="endPointSource"/> is <see langword="null"/>.
        /// </exception>
        public UdpTransport(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        
        /// <inheritdoc />
        public void Send(in Data metric)
        {
            var endpoint = _endpointSource.GetEndpoint();

            using (var socket = CreateSocket())
            {
#if NETCOREAPP2_1
                socket.Connect(endpoint);
                socket.Send(metric.GetSpan());
#else
                socket.SendTo(metric.GetArray(), endpoint);
#endif
            }
        }

        internal static Socket CreateSocket()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
#if !NET451
            // See https://github.com/dotnet/corefx/pull/17853#issuecomment-291371266
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                socket.SendBufferSize = 0;
            }
#else
            socket.SendBufferSize = 0;
#endif
            return socket;
        }
    }
}
