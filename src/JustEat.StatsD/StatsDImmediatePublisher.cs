using System;
using System.Globalization;

namespace JustEat.StatsD
{
    /// <summary>
    ///     Will synchronously publish stats at statsd as you make calls; will not batch sends.
    /// </summary>
    public class StatsDImmediatePublisher : IStatsDPublisher
    {
        private static readonly CultureInfo SafeDefaultCulture = new CultureInfo(StatsDMessageFormatter.SafeDefaultIsoCultureId);
        private readonly StatsDMessageFormatter _formatter;
        private readonly IStatsDUdpClient _transport;
        private bool _disposed;

        public StatsDImmediatePublisher(CultureInfo cultureInfo, string hostNameOrAddress, int port = 8125, string prefix = "")
        {
            _formatter = new StatsDMessageFormatter(cultureInfo, prefix);
            _transport = new StatsDUdpClient(hostNameOrAddress, port);
        }

        public StatsDImmediatePublisher(string hostNameOrAddress, int port = 8125, string prefix = "") : this(SafeDefaultCulture, hostNameOrAddress, port, prefix) {}

        public void Increment(string bucket)
        {
            _transport.Send(_formatter.Increment(bucket));
        }

        public void Increment(long value, string bucket)
        {
            _transport.Send(_formatter.Increment(value, bucket));
        }

        public void Increment(long value, double sampleRate, string bucket)
        {
            _transport.Send(_formatter.Increment(value, sampleRate, bucket));
        }

        public void Increment(long value, double sampleRate, params string[] buckets)
        {
            _transport.Send(_formatter.Increment(value, sampleRate, buckets));
        }

        public void Decrement(string bucket)
        {
            _transport.Send(_formatter.Decrement(bucket));
        }

        public void Decrement(long value, string bucket)
        {
            _transport.Send(_formatter.Decrement(value, bucket));
        }

        public void Decrement(long value, double sampleRate, string bucket)
        {
            _transport.Send(_formatter.Decrement(value, sampleRate, bucket));
        }

        public void Decrement(long value, double sampleRate, params string[] buckets)
        {
            _transport.Send(_formatter.Decrement(value, sampleRate, buckets));
        }

        public void Gauge(long value, string bucket)
        {
            _transport.Send(_formatter.Gauge(value, bucket));
        }

        public void Gauge(long value, string bucket, DateTime timestamp)
        {
            _transport.Send(_formatter.Gauge(value, bucket, timestamp));
        }

        public void Timing(TimeSpan duration, string bucket)
        {
            _transport.Send(_formatter.Timing(Convert.ToInt64(duration.TotalMilliseconds), bucket));
        }

        public void Timing(TimeSpan duration, double sampleRate, string bucket)
        {
            _transport.Send(_formatter.Timing(Convert.ToInt64(duration.TotalMilliseconds), sampleRate, bucket));
        }

        public void MarkEvent(string name)
        {
            _transport.Send(_formatter.Event(name));
        }

        /// <summary>	Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>	Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
        /// <param name="disposing">	true if resources should be disposed, false if not. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != _transport)
                {
                    _transport.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
