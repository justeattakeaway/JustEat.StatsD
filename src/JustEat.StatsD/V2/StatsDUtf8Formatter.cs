using System;
using System.Text;

namespace JustEat.StatsD.V2
{
    internal class StatsDUtf8Formatter
    {
        private readonly byte[] _utf8Prefix;

        public StatsDUtf8Formatter(string prefix)
        {
            _utf8Prefix = string.IsNullOrWhiteSpace(prefix) ? new byte[0] : Encoding.UTF8.GetBytes(prefix + ".");
        }

        public int GetBufferSize(in StatsDMessage msg)
        {
            const int MaxSerializedDoubleSymbols = 32;
            const int MaxUtf8BytesPerChar = 4;
            const int ColonBytes = 1;

            const int MaxMessageKindSuffixSize = 3;
            const int MaxSamplingSuffixSize = 2;

            return _utf8Prefix.Length + (msg.StatBucket.Length * MaxUtf8BytesPerChar)
                + ColonBytes
                + MaxSerializedDoubleSymbols
                + MaxMessageKindSuffixSize
                + MaxSamplingSuffixSize
                + MaxSerializedDoubleSymbols;
        }

        public bool TryFormat(in StatsDMessage msg, double sampleRate, Span<byte> destination, out int written)
        {
            // prefix + msg.Bucket + ":" + msg.Value + (<oneOf> "|ms", "|c", "|g") + {<optional> "|@" + sampleRate }

            written = 0;
            var buffer = new Buffer(destination);

            if (!buffer.TryWriteBytes(_utf8Prefix)) return false;
            if (!buffer.TryWriteUtf8String(msg.StatBucket)) return false;
            if (!buffer.TryWriteByte((byte)':')) return false;

            var magnitudeIntegral = (long) msg.Magnitude;

            switch (msg.MessageKind)
            {
                case StatsDMessageKind.Counter:
                {
                    if (!buffer.TryWriteInt64(magnitudeIntegral)) return false;
                    if (!buffer.TryWriteBytes((byte)'|', (byte)'c')) return false;
                    break;
                }
                case StatsDMessageKind.Timing:
                {
                    if (!buffer.TryWriteInt64(magnitudeIntegral)) return false;
                    if (!buffer.TryWriteBytes((byte)'|', (byte)'m', (byte)'s')) return false;
                    break;
                }
                case StatsDMessageKind.Gauge:
                {
                    if (msg.Magnitude == (double)magnitudeIntegral)
                    {
                        if (!buffer.TryWriteInt64(magnitudeIntegral)) return false;
                    }
                    else
                    {
                        if (!buffer.TryWriteDouble(msg.Magnitude)) return false;;
                    }

                    if (!buffer.TryWriteBytes((byte)'|', (byte)'g')) return false;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(msg.MessageKind), "Unknown StatsD message kind encountered");
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
