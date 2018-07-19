using System;
using System.Net.Sockets;
using System.Text;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class representing an implementation of <see cref="IStatsDTransport"/> uses UDP and pools sockets. This class cannot be inherited.
    /// </summary>
    public sealed class PooledUdpTransport : IStatsDTransport, IDisposable
    {
        private readonly SimpleObjectPool<Socket> _socketPool
            = new SimpleObjectPool<Socket>(
                Environment.ProcessorCount,
                pool => UdpTransport.CreateSocket());

        private readonly IPEndPointSource _endpointSource;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledUdpTransport"/> class.
        /// </summary>
        /// <param name="endPointSource">The <see cref="IPEndPointSource"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="endPointSource"/> is <see langword="null"/>.
        /// </exception>
        public PooledUdpTransport(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
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
            try
            {
                socket.SendTo(bytes, endpoint);
            }
            catch (Exception)
            {
                socket.Dispose();
                throw;
            }

            _socketPool.Push(socket);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PooledUdpTransport"/> class.
        /// </summary>
        ~PooledUdpTransport()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true" /> to release both managed and unmanaged resources;
        /// <see langword="false" /> to release only unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    while (_socketPool.Count > 0)
                    {
                        var socket = _socketPool.Pop();
                        socket?.Dispose();
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
    }
}
