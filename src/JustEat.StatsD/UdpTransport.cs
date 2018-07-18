using System;
using System.Net.Sockets;
using System.Text;
#if !NET451
using System.Runtime.InteropServices;
#endif
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class representing an implementation of <see cref="IStatsDTransport"/> uses UDP.
    /// </summary>
    public class UdpTransport : IStatsDTransport, IDisposable
    {
        private const int PoolSize = 30;

        private readonly SimpleObjectPool<Socket> _socketPool
            = new SimpleObjectPool<Socket>(PoolSize, pool => CreateSocket());

        private readonly IPEndPointSource _endpointSource;
        private bool _disposed;

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

        /// <summary>
        /// Finalizes an instance of the <see cref="UdpTransport"/> class.
        /// </summary>
        ~UdpTransport()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Send(string metric)
        {
            if (string.IsNullOrWhiteSpace(metric))
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(metric);
            var endpoint = _endpointSource.GetEndpoint();

            var socket = _socketPool.Pop();

            if (socket == null)
            {
                return;
            }

            try
            {
                socket.SendTo(bytes, endpoint);
            }
            finally
            {
                _socketPool.Push(socket);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                Socket socket = null;

                try
                {
                    while ((socket = _socketPool.Pop()) != null)
                    {
                        socket.Dispose();
                    }
                }
                catch (ObjectDisposedException)
                {
                    // If the pool has already been disposed by the finalizer, ignore the exception
                    if (disposing)
                    {
                        throw;
                    }
                }

                _disposed = true;
            }
        }

        private static Socket CreateSocket()
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
