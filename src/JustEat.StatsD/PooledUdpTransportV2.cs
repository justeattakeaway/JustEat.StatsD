using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class representing an implementation of <see cref="IStatsDTransport"/> uses UDP and pools sockets. This class cannot be inherited.
    /// </summary>
    public sealed class PooledUdpTransportV2 : IDisposable
    {
        private ConnectedSocketPool _pool;
        private readonly IPEndPointSource _endpointSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledUdpTransport"/> class.
        /// </summary>
        /// <param name="endPointSource">The <see cref="IPEndPointSource"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="endPointSource"/> is <see langword="null"/>.
        /// </exception>
        public PooledUdpTransportV2(IPEndPointSource endPointSource)
        {
            _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));
        }

        public void Send(ArraySegment<byte> metric)
        {
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

    public sealed class SenderV2 : IStatsDPublisher
    {
        private const int DefaultSampleRate = 1;

        private static int _bufferSize = 512;

        [ThreadStatic]
        private static byte[] _buffer;
        private static byte[] Buffer() => _buffer ?? (_buffer = new byte[_bufferSize]);

        [ThreadStatic]
        private static Random _random;
        private static Random Random() => _random ?? (_random = new Random());

        private readonly StatsDUtf8Formatter _formatter;
        private readonly PooledUdpTransportV2 _transport;

        public SenderV2(StatsDConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var endpointSource = EndpointParser.MakeEndPointSource(
                configuration.Host, configuration.Port, configuration.DnsLookupInterval);

            _transport = new PooledUdpTransportV2(endpointSource);

            _formatter = new StatsDUtf8Formatter(configuration.Prefix);
        }

        public void Increment(string bucket) => Increment(1, bucket);
        public void Increment(long value, string bucket) => Increment(value, DefaultSampleRate, bucket);

        public void Increment(long value, double sampleRate, string bucket)
        {
            if (sampleRate >= 1 || sampleRate >= Random().NextDouble())
            {
                var msg = StatsDMessage.Counter(value, bucket);
                SendMessage(sampleRate, msg);
            }
        }

        public void Increment(long value, double sampleRate, params string[] buckets)
        {
            foreach (var bucket in buckets)
            {
                Increment(value, sampleRate, bucket);
            }
        }

        public void Decrement(string bucket) => Increment(-1, DefaultSampleRate, bucket);
        public void Decrement(long value, string bucket) => Increment(value > 0 ? -value : value, DefaultSampleRate, bucket);
        public void Decrement(long value, double sampleRate, string bucket) => Increment(value > 0 ? -value : value, sampleRate, bucket);

        public void Decrement(long value, double sampleRate, params string[] buckets)
        {
            foreach (var bucket in buckets)
            {
                 Increment(value > 0 ? -value : value, sampleRate, bucket);
            }
        }

        public void Gauge(double value, string bucket)
        {
            var msg = StatsDMessage.Gauge(value, bucket);
            SendMessage(DefaultSampleRate, msg);
        }

        public void Gauge(double value, string bucket, DateTime timestamp)
        {
            Gauge(value, bucket);
        }

        public void Gauge(long value, string bucket)
        {
            var msg = StatsDMessage.Gauge(value, bucket);
            SendMessage(DefaultSampleRate, msg);
        }

        public void Gauge(long value, string bucket, DateTime timestamp)
        {
            Gauge(value, bucket);
        }

        public void Timing(TimeSpan duration, string bucket)
        {
            Timing(duration.Ticks, bucket);
        }

        public void Timing(TimeSpan duration, double sampleRate, string bucket)
        {
            Timing(duration.Ticks, sampleRate, bucket);
        }

        public void Timing(long duration, string bucket)
        {
            Timing(duration, DefaultSampleRate, bucket);
        }

        public void Timing(long duration, double sampleRate, string bucket)
        {
            var msg = StatsDMessage.Gauge(duration, bucket);
            SendMessage(sampleRate, msg);
        }

        public void MarkEvent(string name)
        {
            Increment(name);
        }

        private void SendMessage(double sampleRate, StatsDMessage msg)
        {
            var destination = Buffer();

            if (_formatter.TryFormat(msg, sampleRate, destination, out int written))
            {
                _transport.Send(new ArraySegment<byte>(destination, 0, written));
            }
            else
            {
                var newSize = _formatter.GetBufferSize(msg);
                Interlocked.CompareExchange(ref _bufferSize, destination.Length, newSize);

                if (_formatter.TryFormat(msg, sampleRate, destination, out written))
                {
                    _transport.Send(new ArraySegment<byte>(destination, 0, written));
                }
                else
                {
                    throw new Exception();
                }
            }
        }
    }
}
