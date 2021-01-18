using System;
using System.Collections.Generic;

namespace JustEat.StatsD.Buffered
{
    internal sealed class BufferBasedStatsDPublisher : IStatsDPublisher
    {
        private const double DefaultSampleRate = 1.0;
        private const int SafeUdpPacketSize = 512;

        [ThreadStatic]
        private static byte[]? _buffer;
        private static byte[] Buffer => _buffer ??= new byte[SafeUdpPacketSize];

#pragma warning disable CA5394
        [ThreadStatic]
        private static Random? _random;
        private static Random Random => _random ??= new Random();
#pragma warning disable CA5394

        private readonly StatsDUtf8Formatter _formatter;
        private readonly IStatsDTransport _transport;
        private readonly Func<Exception, bool>? _onError;

        internal BufferBasedStatsDPublisher(StatsDConfiguration configuration, IStatsDTransport transport)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _onError = configuration.OnError;
            _transport = transport;
            _formatter = new StatsDUtf8Formatter(configuration.Prefix, configuration.TagsFormatter);
        }

        public void Increment(long value, double sampleRate, string bucket, Dictionary<string, string?>? tags)
        {
            SendMessage(sampleRate, StatsDMessage.Counter(value, bucket, tags));
        }

        public void Gauge(double value, string bucket, Dictionary<string, string?>? tags)
        {
            SendMessage(DefaultSampleRate, StatsDMessage.Gauge(value, bucket, tags));
        }

        public void Timing(long duration, double sampleRate, string bucket, Dictionary<string, string?>? tags)
        {
            SendMessage(sampleRate, StatsDMessage.Timing(duration, bucket, tags));
        }

        private void SendMessage(double sampleRate, in StatsDMessage msg)
        {
            bool shouldSendMessage = (sampleRate >= DefaultSampleRate || sampleRate > Random.NextDouble()) && msg.StatBucket != null;

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
                        throw new InvalidOperationException("Failed to format buffer to UTF-8.");
                    }
                }
            }
#pragma warning disable CA2201
            catch (Exception ex)
#pragma warning restore CA2201
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
