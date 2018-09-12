using System;
using JustEat.StatsD.EndpointLookups;
using JustEat.StatsD.V2;

namespace JustEat.StatsD
{
    /// <summary>
    ///     Will synchronously publish stats at statsd as you make calls; will not batch sends.
    /// </summary>
    public class StatsDPublisher : IStatsDPublisher
    {
        private readonly IStatsDPublisher _publisher;

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
                case IStatsDTransportV2 transportV2:
                    _publisher = new BufferBasedStatsDPublisher(configuration, transportV2);
                    break;
                default:
                    _publisher = new StringBasedStatsDPublisher(configuration, transport);
                    break;
            }
        }

        public StatsDPublisher(StatsDConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var endpointSource = EndpointParser.MakeEndPointSource(
                configuration.Host, configuration.Port, configuration.DnsLookupInterval);

            var transport = new PooledUdpTransport(endpointSource);

            _publisher = new BufferBasedStatsDPublisher(configuration, transport);
        }

        public void MarkEvent(string name) => _publisher.MarkEvent(name);

        public void Increment(string bucket) => _publisher.Increment(bucket);
        public void Increment(long value, string bucket) => _publisher.Increment(value, bucket);
        public void Increment(long value, double sampleRate, string bucket) => _publisher.Increment(value, sampleRate, bucket);
        public void Increment(long value, double sampleRate, params string[] buckets) => _publisher.Increment(value, sampleRate, buckets);

        public void Decrement(string bucket) => _publisher.Decrement(bucket);
        public void Decrement(long value, string bucket) => _publisher.Decrement(value, bucket);
        public void Decrement(long value, double sampleRate, string bucket) => _publisher.Decrement(value, sampleRate, bucket);
        public void Decrement(long value, double sampleRate, params string[] buckets) => _publisher.Decrement(value, sampleRate, buckets);

        public void Gauge(double value, string bucket) => _publisher.Gauge(value, bucket);
        public void Gauge(double value, string bucket, DateTime timestamp) => _publisher.Gauge(value, bucket, timestamp);
        public void Gauge(long value, string bucket) => _publisher.Gauge(value, bucket);
        public void Gauge(long value, string bucket, DateTime timestamp) => _publisher.Gauge(value, bucket, timestamp);

        public void Timing(TimeSpan duration, string bucket) => _publisher.Timing(duration, bucket);
        public void Timing(TimeSpan duration, double sampleRate, string bucket) => _publisher.Timing(duration, sampleRate, bucket);
        public void Timing(long duration, string bucket) => _publisher.Timing(duration, bucket);
        public void Timing(long duration, double sampleRate, string bucket) => _publisher.Timing(duration, sampleRate, bucket);
    }
}
