using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace JustEat.StatsD
{
    internal sealed class ConnectedSocketPool : IDisposable
    {
        private bool _disposed;
        private readonly ConcurrentBag<Socket> _pool = new ConcurrentBag<Socket>();
        private readonly SocketProtocol _socketProtocol;

        public EndPoint EndPoint { get; }

        public ConnectedSocketPool(EndPoint endPoint, SocketProtocol socketProtocol, int initialSize)
        {
            EndPoint = endPoint;
            _socketProtocol = socketProtocol;
            PrePopulateSocketPool(initialSize);
        }

        private void PrePopulateSocketPool(int initialSize)
        {
            while (_pool.Count < initialSize)
            {
                _pool.Add(CreateSocket());
            }
        }

        private Socket CreateSocket()
        {
            var socket = SocketFactory.For(_socketProtocol);
            try
            {
                socket.Connect(EndPoint);
                return socket;
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        /// <summary>Retrieves an object from the pool if one is available.
        /// return null if the pool is empty</summary>
        /// <returns>An object from the pool. </returns>
        private Socket? Pop()
        {
            if (_pool.TryTake(out Socket result))
            {
                return result;
            }

            return null;
        }

        /// <summary>Retrieves an Socket from the pool if one is available.
        /// Creates a new Socket if the pool is empty </summary>
        /// <returns>A Socket from the pool. </returns>
        internal Socket PopOrCreate()
        {
            return Pop() ?? CreateSocket();
        }

        /// <summary>Pushes an object back into the pool. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when the item is null.</exception>
        /// <param name="item">The Socket to push.</param>
        public void Push(Socket item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), $"An item added to a {nameof(ConnectedSocketPool)} cannot be null.");
            }

            _pool.Add(item);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ConnectedSocketPool"/> class.
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
                    while (_pool.Count > 0)
                    {
                        var socket = Pop();
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
