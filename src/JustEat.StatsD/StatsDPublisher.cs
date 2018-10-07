using System;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    ///     Will synchronously publish stats at statsd as you make calls; will not batch sends.
    /// </summary>
    public sealed class StatsDPublisher : IStatsDPublisher, IDisposable
    {
        private readonly IStatsDPublisher _inner;
        private readonly IStatsDTransport _transport;

        private bool _disposed;
        private bool _disposeTransport;

        public StatsDPublisher(StatsDConfiguration configuration, IStatsDTransport transport)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (transport == null)
            {
                throw new ArgumentNullException(nameof(transport));
            }

            switch (transport)
            {
                case IStatsDBufferedTransport bufferedTransport when configuration.PreferBufferedTransport:
                    _inner = new BufferBasedStatsDPublisher(configuration, bufferedTransport);
                    break;
                default:
                    _inner = new StringBasedStatsDPublisher(configuration, transport);
                    break;
            }

            _transport = transport;
            _disposeTransport = false;
        }

        public StatsDPublisher(StatsDConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var endpointSource = EndpointParser.MakeEndPointSource(
                configuration.Host, configuration.Port, configuration.DnsLookupInterval);

            var transport = new SocketTransport(endpointSource, configuration.SocketProtocol);

            _transport = transport;
            _disposeTransport = true;

            if (configuration.PreferBufferedTransport)
            {
                _inner = new BufferBasedStatsDPublisher(configuration, transport);
            }
            else
            {
                _inner = new StringBasedStatsDPublisher(configuration, transport);
            }
        }

        public void MarkEvent(string name)
        {
            _inner.MarkEvent(name);
        }

        public void Increment(string bucket)
        {
            _inner.Increment(bucket);
        }

        public void Increment(long value, string bucket)
        {
            _inner.Increment(value, bucket);
        }

        public void Increment(long value, double sampleRate, string bucket)
        {
            _inner.Increment(value, sampleRate, bucket);
        }

        public void Increment(long value, double sampleRate, params string[] buckets)
        {
            _inner.Increment(value, sampleRate, buckets);
        }

        public void Decrement(string bucket)
        {
            _inner.Decrement(bucket);
        }

        public void Decrement(long value, string bucket)
        {
            _inner.Decrement(value, bucket);
        }

        public void Decrement(long value, double sampleRate, string bucket)
        {
            _inner.Decrement(value, sampleRate, bucket);
        }

        public void Decrement(long value, double sampleRate, params string[] buckets)
        {
            _inner.Decrement(value, sampleRate, buckets);
        }

        public void Gauge(double value, string bucket)
        {
            _inner.Gauge(value, bucket);
        }

        public void Gauge(long value, string bucket)
        {
            _inner.Gauge(value, bucket);
        }

        public void Timing(TimeSpan duration, string bucket)
        {
            _inner.Timing(duration, bucket);
        }

        public void Timing(TimeSpan duration, double sampleRate, string bucket)
        {
            _inner.Timing(duration, sampleRate, bucket);
        }

        public void Timing(long duration, string bucket)
        {
            _inner.Timing(duration, bucket);
        }

        public void Timing(long duration, double sampleRate, string bucket)
        {
            _inner.Timing(duration, sampleRate, bucket);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_disposeTransport && _transport is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
