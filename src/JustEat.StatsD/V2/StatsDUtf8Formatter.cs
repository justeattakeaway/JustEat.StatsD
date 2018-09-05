using System;
using System.Text;

namespace JustEat.StatsD.V2
{
    internal class StatsDUtf8Formatter
    {
        private readonly byte[] _utf8Prefix;

        public StatsDUtf8Formatter(string prefix = "")
        {
            _utf8Prefix = string.IsNullOrWhiteSpace(prefix) ? new byte[0] : Encoding.UTF8.GetBytes(prefix + ".");
        }

        public int GetBufferSize(in StatsDMessage msg)
        {
            const int maxSerializedDoubleSymbols = 20;
            const int maxUtf8BytesPerChar = 4;
            const int colonBytes = 1;

            const int maxMessageKindSuffixSize = 3;
            const int maxSamplingSuffixSize = 2;

            return _utf8Prefix.Length + msg.StatBucket.Length * maxUtf8BytesPerChar
              + colonBytes
              + maxSerializedDoubleSymbols
              + maxMessageKindSuffixSize
              + maxSamplingSuffixSize
              + maxSerializedDoubleSymbols;
        }

        // prefix + msg.Bucket + ":" + msg.Value + (<oneOf> "|ms", "|c", "|g") + {<optional> "|@" + sampleRate }
        public bool TryFormat(in StatsDMessage msg, double sampleRate, Span<byte> destination, out int written)
        {
            written = 0;
            var buffer = new Buffer(destination);

            if (!buffer.TryWriteBytes(_utf8Prefix)) return false;
            if (!buffer.TryWriteUtf8String(msg.StatBucket)) return false;
            if (!buffer.TryWriteBytes((byte)':')) return false;

            var magnitudeIntegral = (long) msg.Magnitude;

            switch (msg.MessageKind)
            {
                case StatsDMessage.Kind.Counter:
                {
                    if (!buffer.TryWriteLong(magnitudeIntegral)) return false;
                    if (!buffer.TryWriteBytes((byte)'|', (byte)'c')) return false;
                    break;
                }
                case StatsDMessage.Kind.Timing:
                {
                    if (!buffer.TryWriteLong(magnitudeIntegral)) return false;
                    if (!buffer.TryWriteBytes((byte)'|', (byte)'m', (byte)'s')) return false;
                    break;
                }
                case StatsDMessage.Kind.Gauge:
                {
                    if (msg.Magnitude == (double)magnitudeIntegral)
                    {
                        if (!buffer.TryWriteLong(magnitudeIntegral)) return false;
                    }
                    else
                    {
                        if (!buffer.TryWriteDouble(msg.Magnitude)) return false;;
                    }

                    if (!buffer.TryWriteBytes((byte)'|', (byte)'g')) return false;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (sampleRate < 1.0 && sampleRate > 0.0)
            {
                if (!buffer.TryWriteBytes((byte)'|', (byte)'@')) return false;
                if (!buffer.TryWriteDouble(sampleRate)) return false;
            }

            written = buffer.Written;
            return true;
        }
        
    }
}
