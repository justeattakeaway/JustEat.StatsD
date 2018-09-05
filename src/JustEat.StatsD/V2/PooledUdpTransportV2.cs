using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD.V2
{
    public sealed class PooledUdpTransportV2 : IDisposable, IStatsDTransportV2
    {
        private ConnectedSocketPool _pool;
        private readonly IPEndPointSource _endpointSource;

        public PooledUdpTransportV2(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        public void Send(ArraySegment<byte> metric)
        {
            if (metric.Array == null)
                return;

            var pool = GetPool(_endpointSource.GetEndpoint());
            var socket = pool.PopOrCreate();

            try
            {
                socket.Send(metric.Array, metric.Offset, metric.Count, SocketFlags.None);
            }
            catch (Exception)
            {
                socket.Dispose();
                throw;
            }

            pool.Push(socket);
        }

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
                var newPool = new ConnectedSocketPool(endPoint);

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
