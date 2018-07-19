using System;
using System.Net;
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
        private sealed class ConnectedPool : IDisposable
        {
            private bool _disposed;

            public ConnectedPool(IPEndPoint ipEndPoint)
            {
                IpEndPoint = ipEndPoint;

                Pool = new SimpleObjectPool<Socket>(
                    Environment.ProcessorCount,
                    pool =>
                    {
                        var socket = UdpTransport.CreateSocket();
                        socket.Connect(ipEndPoint);
                        return socket;
                    });
            }

            public SimpleObjectPool<Socket> Pool { get; }

            public IPEndPoint IpEndPoint { get; }

            /// <summary>
            /// Finalizes an instance of the <see cref="PooledUdpTransport"/> class.
            /// </summary>
            ~ConnectedPool()
            {
                Dispose(false);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    try
                    {
                        while (Pool.Count > 0)
                        {
                            var socket = Pool.Pop();
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

        private ConnectedPool _pool;
        private readonly IPEndPointSource _endpointSource;

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
            var pool = GetPool(endpoint);
            var socket = pool.Pool.PopOrCreate();

            try
            {
                socket.Send(bytes);
            }
            catch (Exception)
            {
                socket.Dispose();
                throw;
            }

            pool.Pool.Push(socket);
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            _pool?.Dispose();
        }

        private readonly object _lock = new object();

        private ConnectedPool GetPool(IPEndPoint endPoint)
        {
            if (_pool == null)
            {
                lock (_lock)
                {
                    if (_pool == null)
                    {
                        _pool = new ConnectedPool(endPoint);
                        return _pool;
                    }
                }
            }

            if (endPoint.Equals(_pool.IpEndPoint))
            {
                return _pool;
            }
            else
            {
                lock (_lock)
                {
                    _pool.Dispose();
                    _pool = new ConnectedPool(endPoint);
                    return _pool;
                }
            }

        }
    }
}
