using System;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class representing the default implementation of <see cref="IStatsDPublisher"/> that
    /// publishes counters, gauges and timers to statsD. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// Metrics are published synchronously immediately and are not batched.
    /// </remarks>
    public sealed class StatsDPublisher : IStatsDPublisher, IDisposable
    {
        private readonly IStatsDPublisher _inner;
        private readonly IStatsDTransport _transport;

        private bool _disposed;
        private bool _disposeTransport;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatsDPublisher"/> class for the specified transport.
        /// </summary>
        /// <param name="configuration">The statsD configuration to use.</param>
        /// <param name="transport">The transport implementation to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="configuration"/> or <paramref name="transport"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="configuration"/> is invalid, such as an invalid hostname or IP address.
        /// </exception>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="StatsDPublisher"/> class using the default transport.
        /// </summary>
        /// <param name="configuration">The statsD configuration to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="configuration"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="configuration"/> is invalid, such as an invalid hostname or IP address.
        /// </exception>
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

        /// <inheritdoc />
        public void MarkEvent(string name)
        {
            _inner.MarkEvent(name);
        }

        /// <inheritdoc />
        public void Increment(string bucket)
        {
            _inner.Increment(bucket);
        }

        /// <inheritdoc />
        public void Increment(long value, string bucket)
        {
            _inner.Increment(value, bucket);
        }

        /// <inheritdoc />
        public void Increment(long value, double sampleRate, string bucket)
        {
            _inner.Increment(value, sampleRate, bucket);
        }

        /// <inheritdoc />
        public void Increment(long value, double sampleRate, params string[] buckets)
        {
            _inner.Increment(value, sampleRate, buckets);
        }

        /// <inheritdoc />
        public void Decrement(string bucket)
        {
            _inner.Decrement(bucket);
        }

        /// <inheritdoc />
        public void Decrement(long value, string bucket)
        {
            _inner.Decrement(value, bucket);
        }

        /// <inheritdoc />
        public void Decrement(long value, double sampleRate, string bucket)
        {
            _inner.Decrement(value, sampleRate, bucket);
        }

        /// <inheritdoc />
        public void Decrement(long value, double sampleRate, params string[] buckets)
        {
            _inner.Decrement(value, sampleRate, buckets);
        }

        /// <inheritdoc />
        public void Gauge(double value, string bucket)
        {
            _inner.Gauge(value, bucket);
        }

        /// <inheritdoc />
        public void Gauge(long value, string bucket)
        {
            _inner.Gauge(value, bucket);
        }

        /// <inheritdoc />
        public void Timing(TimeSpan duration, string bucket)
        {
            _inner.Timing(duration, bucket);
        }

        /// <inheritdoc />
        public void Timing(TimeSpan duration, double sampleRate, string bucket)
        {
            _inner.Timing(duration, sampleRate, bucket);
        }

        /// <inheritdoc />
        public void Timing(long duration, string bucket)
        {
            _inner.Timing(duration, bucket);
        }

        /// <inheritdoc />
        public void Timing(long duration, double sampleRate, string bucket)
        {
            _inner.Timing(duration, sampleRate, bucket);
        }

        /// <inheritdoc />
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
