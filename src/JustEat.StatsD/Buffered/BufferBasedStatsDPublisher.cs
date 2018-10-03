using System;

namespace JustEat.StatsD.Buffered
{
    internal sealed class BufferBasedStatsDPublisher : IStatsDPublisher
    {
        private const double DefaultSampleRate = 1.0;
        private const int SafeUdpPacketSize = 512;

        [ThreadStatic]
        private static byte[] _buffer;
        private static byte[] Buffer => _buffer ?? (_buffer = new byte[SafeUdpPacketSize]);

        [ThreadStatic]
        private static Random _random;
        private static Random Random => _random ?? (_random = new Random());

        private readonly StatsDUtf8Formatter _formatter;
        private readonly IStatsDBufferedTransport _transport;
        private readonly Func<Exception, bool> _onError;

        public BufferBasedStatsDPublisher(StatsDConfiguration configuration, IStatsDBufferedTransport transport)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _onError = configuration.OnError;
            _transport = transport;
            _formatter = new StatsDUtf8Formatter(configuration.Prefix);
        }

        public void Increment(string bucket)
        {
            Increment(1, bucket);
        }

        public void Increment(long value, string bucket)
        {
            Increment(value, DefaultSampleRate, bucket);
        }

        public void Increment(long value, double sampleRate, string bucket)
        {
            var msg = StatsDMessage.Counter(value, bucket);
            SendMessage(sampleRate, msg);
        }

        public void Increment(long value, double sampleRate, params string[] buckets)
        {
            if (buckets == null || buckets.Length == 0)
            {
                return;
            }

            foreach (var bucket in buckets)
            {
                Increment(value, sampleRate, bucket);
            }
        }

        public void Decrement(string bucket)
        {
            Increment(-1, DefaultSampleRate, bucket);
        }

        public void Decrement(long value, string bucket)
        {
            Increment(value > 0 ? -value : value, DefaultSampleRate, bucket);
        }

        public void Decrement(long value, double sampleRate, string bucket)
        {
            Increment(value > 0 ? -value : value, sampleRate, bucket);
        }

        public void Decrement(long value, double sampleRate, params string[] buckets)
        {
            if (buckets == null || buckets.Length == 0)
            {
                return;
            }

            foreach (var bucket in buckets)
            {
                Increment(value > 0 ? -value : value, sampleRate, bucket);
            }
        }

        public void Gauge(double value, string bucket)
        {
            SendMessage(DefaultSampleRate, StatsDMessage.Gauge(value, bucket));
        }

        public void Gauge(double value, string bucket, DateTime timestamp)
        {
            Gauge(value, bucket);
        }

        public void Gauge(long value, string bucket)
        {
            Gauge((double) value, bucket);
        }

        public void Gauge(long value, string bucket, DateTime timestamp)
        {
            Gauge(value, bucket);
        }

        public void Timing(TimeSpan duration, string bucket)
        {
            Timing((long)duration.TotalMilliseconds, bucket);
        }

        public void Timing(TimeSpan duration, double sampleRate, string bucket)
        {
            Timing((long)duration.TotalMilliseconds, sampleRate, bucket);
        }

        public void Timing(long duration, string bucket)
        {
            Timing(duration, DefaultSampleRate, bucket);
        }

        public void Timing(long duration, double sampleRate, string bucket)
        {
            var msg = StatsDMessage.Timing(duration, bucket);
            SendMessage(sampleRate, msg);
        }

        public void MarkEvent(string name)
        {
            Increment(name);
        }

        private void SendMessage(double sampleRate, in StatsDMessage msg)
        {
            bool shouldSendMessage = sampleRate >= DefaultSampleRate || sampleRate > Random.NextDouble();

            if (!shouldSendMessage)
            {
                return;
            }

            try
            {
                var buffer = Buffer;

                if (_formatter.TryFormat(msg, sampleRate, buffer, out int written))
                {
                    _transport.Send(new ArraySegment<byte>(buffer, 0, written));
                }
                else
                {
                    var newSize = _formatter.GetMaxBufferSize(msg);
                    
                    _buffer = new byte[newSize];

                    if (_formatter.TryFormat(msg, sampleRate, _buffer, out written))
                    {
                        _transport.Send(new ArraySegment<byte>(_buffer, 0, written));
                    }
                    else
                    {
                        throw new Exception("Utf8 Formatting Error");
                    }
                }
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
