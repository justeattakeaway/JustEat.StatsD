using System;
using System.Net;
using System.Net.Sockets;

namespace JustEat.StatsD
{
    internal sealed class ConnectedSocketPool : IDisposable
    {
        private bool _disposed;
        private readonly SimpleObjectPool<Socket> _pool;

        public ConnectedSocketPool(IPEndPoint ipEndPoint)
        {
            IpEndPoint = ipEndPoint;

            _pool = new SimpleObjectPool<Socket>(
                Environment.ProcessorCount,
                pool =>
                {
                    var socket = UdpTransport.CreateSocket();
                    try
                    {
                        socket.Connect(ipEndPoint);
                        return socket;
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                });
        }

        public IPEndPoint IpEndPoint { get; }

        public Socket PopOrCreate()
        {
            return _pool.PopOrCreate();
        }

        public void Push(Socket item)
        {
            _pool.Push(item);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PooledUdpTransport"/> class.
        /// </summary>
        ~ConnectedSocketPool()
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
                    while (_pool?.Count > 0)
                    {
                        var socket = _pool.Pop();
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
