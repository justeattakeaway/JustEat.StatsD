using System;
using System.Text;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
    /// <summary>
    ///     Will synchronously publish stats at statsd as you make calls; will not batch sends.
    /// </summary>
    public class StatsDPublisher : IStatsDPublisher
    {
        private readonly StatsDMessageFormatter _formatter;
        private readonly IStatsDTransport _transport;
        private readonly Func<Exception, bool> _onError;
        private readonly SpanStatsDMessageFormatter _spanFormatter;

        public StatsDPublisher(StatsDConfiguration configuration, IStatsDTransport transport)
        {
            if (configuration == null)
            {
               throw new ArgumentNullException(nameof(configuration));
            }

            _transport = transport ?? throw new ArgumentNullException(nameof(transport));

            _formatter = new StatsDMessageFormatter(configuration.Prefix);
            _spanFormatter = new SpanStatsDMessageFormatter(configuration.Prefix);
            _onError = configuration.OnError;
        }

        public StatsDPublisher(StatsDConfiguration configuration)
        {
            if (configuration == null)
            {
               throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(configuration.Host))
            {
                throw new ArgumentNullException(nameof(configuration.Host));
            }

            _formatter = new StatsDMessageFormatter(configuration.Prefix);

            var endpointSource = EndpointParser.MakeEndPointSource(
                configuration.Host, configuration.Port, configuration.DnsLookupInterval);
            _transport = new UdpTransport(endpointSource);
            _onError = configuration.OnError;
        }

        public void Increment(string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Increment(bucket, ref writer);
            Send(writer.Get());
        }

        public void Increment(long value, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Increment(value, bucket, ref writer);
            Send(writer.Get());
        }

        public void Increment(long value, double sampleRate, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Increment(value, sampleRate, bucket, ref writer);
            Send(writer.Get());
        }

        public void Increment(long value, double sampleRate, params string[] buckets)
        {
            Send(Encoding.UTF8.GetBytes(_formatter.Increment(value, sampleRate, buckets)));
        }

        public void Decrement(string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Decrement(bucket, ref writer);
            Send(writer.Get());
        }

        public void Decrement(long value, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Decrement(value, bucket, ref writer);
            Send(writer.Get());
        }

        public void Decrement(long value, double sampleRate, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Decrement(value, sampleRate, bucket, ref writer);
            Send(writer.Get());
        }

        public void Decrement(long value, double sampleRate, params string[] buckets)
        {
            Send(Encoding.UTF8.GetBytes(_formatter.Decrement(value, sampleRate, buckets)));
        }

        public void Gauge(double  value, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Gauge(value, bucket, ref writer);
            Send(writer.Get());
        }

        public void Gauge(double value, string bucket, DateTime timestamp)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Gauge(value, bucket, timestamp, ref writer);
            Send(writer.Get());
        }

        public void Gauge(long value, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Gauge(value, bucket, ref writer);
            Send(writer.Get());
        }

        public void Gauge(long value, string bucket, DateTime timestamp)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Gauge(value, bucket, timestamp, ref writer);
            Send(writer.Get());
        }

        public void Timing(TimeSpan duration, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Timing(Convert.ToInt64(duration.TotalMilliseconds), bucket, ref writer);
            Send(writer.Get());
        }

        public void Timing(TimeSpan duration, double sampleRate, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Timing(Convert.ToInt64(duration.TotalMilliseconds), sampleRate, bucket, ref writer);
            Send(writer.Get());
        }

        public void Timing(long duration, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Timing(duration, bucket, ref writer);
            Send(writer.Get());
        }

        public void Timing(long duration, double sampleRate, string bucket)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Timing(duration, sampleRate, bucket, ref writer);
            Send(writer.Get());
        }

        public void MarkEvent(string name)
        {
            Span<byte> buffer = stackalloc byte[512];
            var writer = new FixedBuffer(buffer);
            _spanFormatter.Event(name, ref writer);
            Send(writer.Get());
        }

        private void Send(ReadOnlySpan<byte> metric)
        {
            try
            {
                _transport.Send(metric);
            }
            catch (Exception ex)
            {
                var handled = _onError?.Invoke(ex) ?? true;
                if (!handled)
                {
                    throw;
                }
            }
        }
    }
}
