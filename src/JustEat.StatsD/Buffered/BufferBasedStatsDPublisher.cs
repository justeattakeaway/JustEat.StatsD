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
        private readonly IStatsDTransport _transport;
        private readonly Func<Exception, bool> _onError;

        public BufferBasedStatsDPublisher(StatsDConfiguration configuration, IStatsDTransport transport)
        {
            _onError = configuration.OnError;
            _transport = transport;
            _formatter = new StatsDUtf8Formatter(configuration.Prefix);
        }

        public void Increment(long value, double sampleRate, string bucket)
        {
            SendMessage(sampleRate, StatsDMessage.Counter(value, bucket));
        }

        public void Gauge(double value, string bucket)
        {
            SendMessage(DefaultSampleRate, StatsDMessage.Gauge(value, bucket));
        }

        public void Timing(long duration, double sampleRate, string bucket)
        {
            SendMessage(sampleRate, StatsDMessage.Timing(duration, bucket));
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
                        throw new Exception("Failed to format buffer to UTF-8.");
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
