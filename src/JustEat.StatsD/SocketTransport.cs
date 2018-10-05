using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class representing an implementation of <see cref="IStatsDTransport"/>
    /// that uses UDP or IP sockets and pools them.
    /// This class cannot be inherited.
    /// </summary>
    public sealed class SocketTransport : IStatsDTransport, IStatsDBufferedTransport, IDisposable
    {
        private ConnectedSocketPool _pool;
        private readonly IPEndPointSource _endpointSource;
        private readonly SocketProtocol _socketProtocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketTransport"/> class.
        /// </summary>
        /// <param name="endPointSource">The <see cref="IPEndPointSource"/> to use.</param>
        /// <param name="socketProtocol">Udp or Ip sockets</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="endPointSource"/> is <see langword="null"/>.
        /// </exception>
        public SocketTransport(IPEndPointSource endPointSource, SocketProtocol socketProtocol)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));

            if (socketProtocol != SocketProtocol.IP && socketProtocol != SocketProtocol.Udp)
            {
                throw new ArgumentOutOfRangeException(nameof(socketProtocol));
            }
            _socketProtocol = socketProtocol;
        }

        /// <inheritdoc />
        public void Send(string metric)
        {
            if (string.IsNullOrWhiteSpace(metric))
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(metric);
            var pool = GetPool(_endpointSource.GetEndpoint());
            var socket = pool.PopOrCreate();

            try
            {
                socket.Send(bytes);
            }
            catch (Exception)
            {
                socket.Dispose();
                throw;
            }

            pool.Push(socket);
        }

        public void Send(in ArraySegment<byte> metric)
        {
            if (metric.Array == null || metric.Count == 0)
            {
                return;
            }

            var pool = GetPool(_endpointSource.GetEndpoint());
            var socket = pool.PopOrCreate();

            try
            {
                socket.Send(metric.Array, 0, metric.Count, SocketFlags.None);
            }
            catch (Exception)
            {
                socket.Dispose();
                throw;
            }

            pool.Push(socket);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _pool?.Dispose();
        }

        private ConnectedSocketPool GetPool(IPEndPoint endPoint)
        {
            var oldPool = _pool;

            if (oldPool != null && (ReferenceEquals(oldPool.IpEndPoint, endPoint) || oldPool.IpEndPoint.Equals(endPoint)))
            {
                return oldPool;
            }
            else
            {
                var newPool = new ConnectedSocketPool(
                    endPoint, _socketProtocol, Environment.ProcessorCount);

                if (Interlocked.CompareExchange(ref _pool, newPool, oldPool) == oldPool)
                {
                    oldPool?.Dispose();
                    return newPool;
                }
                else
                {
                    newPool.Dispose();
                    return _pool;
                }
            }
        }
    }
}
