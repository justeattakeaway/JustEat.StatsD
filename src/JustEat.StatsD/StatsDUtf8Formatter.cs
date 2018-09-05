using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

[assembly:InternalsVisibleTo("JustEat.StatsD.Tests")]
[assembly:InternalsVisibleTo("Benchmark")]

namespace JustEat.StatsD
{
    internal enum StatsDMessageKind { Counter, Timing, Gauge }

    internal readonly struct StatsDMessage
    {
        public readonly string StatBucket;
        public readonly double Magnitude;
        public readonly StatsDMessageKind MessageKind;

        private StatsDMessage(string statBucket, double magnitude, StatsDMessageKind messageKind)
        {
            StatBucket = statBucket;
            Magnitude = magnitude;
            MessageKind = messageKind;
        }

        public static StatsDMessage Timing(long milliseconds, string statBucket)
        {
            return new StatsDMessage(statBucket, milliseconds, StatsDMessageKind.Timing);
        }

        public static StatsDMessage Counter(long magnitude, string statBucket)
        {
            return new StatsDMessage(statBucket ,magnitude,StatsDMessageKind.Counter);
        }

        public static StatsDMessage Gauge(double magnitude, string statBucket)
        {
            return new StatsDMessage(statBucket, magnitude, StatsDMessageKind.Gauge);
        }
    }

    internal ref struct Buffer
    {
        public Buffer(Span<byte> source) : this()
        {
            Tail = source;
            Written = 0;
        }

        public Span<byte> Tail;
        public int Written;
    }

    internal static class BufferImpl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteBytes(this ref Buffer src, Span<byte> bytes)
        {
            if (bytes.Length > src.Tail.Length) return false;

            bytes.CopyTo(src.Tail);
            src.Tail = src.Tail.Slice(bytes.Length);
            src.Written += bytes.Length;
            return true;
        }

        public static bool TryWriteUtf8String(this ref Buffer src, string str)
        {
#if NETCOREAPP2_1
            try
            {
                var written = Encoding.UTF8.GetBytes(str, src.Tail);
                src.Tail = src.Tail.Slice(written);
                src.Written += written;
                return true;
            }
            catch
            {
                return false;
            }
#else
            var bucketBytes = Encoding.UTF8.GetBytes(str);
            return src.TryWriteBytes(bucketBytes);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteAsciiChars(this ref Buffer src, char chr)
        {
            if (src.Tail.Length < 1) return false;

            src.Tail[0] = (byte) chr;
            src.Tail = src.Tail.Slice(1);
            src.Written += 1;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteAsciiChars(this ref Buffer src, char chr1, char chr2)
        {
            if (src.Tail.Length < 2) return false;

            src.Tail[0] = (byte) chr1;
            src.Tail[1] = (byte) chr2;
            src.Tail = src.Tail.Slice(2);
            src.Written += 2;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteAsciiChars(this ref Buffer src, char chr1, char chr2, char chr3)
        {
            if (src.Tail.Length < 3) return false;

            src.Tail[0] = (byte) chr1;
            src.Tail[1] = (byte) chr2;
            src.Tail[2] = (byte) chr3;
            src.Tail = src.Tail.Slice(3);
            src.Written += 2;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteLong(this ref Buffer src, long val)
        {
            if (Utf8Formatter.TryFormat(val, src.Tail, out var consumed))
            {
                src.Tail = src.Tail.Slice(consumed);
                src.Written += consumed;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteDouble(this ref Buffer src, double val)
        {
            if (Utf8Formatter.TryFormat((decimal)val, src.Tail, out var consumed))
            {
                src.Tail = src.Tail.Slice(consumed);
                src.Written += consumed;
                return true;
            }

            return false;
        }
    }

    internal class StatsDUtf8Formatter
    {
        private readonly byte[] _prefix;

        public StatsDUtf8Formatter(string prefix = "")
        {
            _prefix = string.IsNullOrWhiteSpace(prefix) ? new byte[0] : Encoding.UTF8.GetBytes(prefix + ".");
        }

        public int GetBufferSize(in StatsDMessage msg) =>
            _prefix.Length + msg.StatBucket.Length * 4 + 1 + 20 + 3 + 2 + 20;

        public bool TryFormat(in StatsDMessage msg, double sampleRate, Span<byte> destination, out int written)
        {
            written = 0;
            var buffer = new Buffer(destination);

            if (!buffer.TryWriteBytes(_prefix)) return false;
            if (!buffer.TryWriteUtf8String(msg.StatBucket)) return false;

            if (!buffer.TryWriteAsciiChars(':')) return false;

            switch (msg.MessageKind)
            {
                case StatsDMessageKind.Counter:
                {
                    var magnitude = (long) msg.Magnitude;
                    if (!buffer.TryWriteLong(magnitude)) return false;
                    if (!buffer.TryWriteAsciiChars('|', 'c')) return false;
                    break;
                }
                case StatsDMessageKind.Timing:
                {
                    var magnitude = (long) msg.Magnitude;
                    if (!buffer.TryWriteLong(magnitude)) return false;
                    if (!buffer.TryWriteAsciiChars('|', 'm', 's')) return false;
                    break;
                }
                case StatsDMessageKind.Gauge:
                {
                    var magnitude = (long) msg.Magnitude;
                    if (msg.Magnitude == magnitude)
                    {
                        if (!buffer.TryWriteLong(magnitude)) return false;
                    }
                    else
                    {
                        if (!buffer.TryWriteDouble(msg.Magnitude)) return false;;
                    }

                    if (!buffer.TryWriteAsciiChars('|', 'g')) return false;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (sampleRate < 1.0 && sampleRate > 0.0)
            {
                if (!buffer.TryWriteAsciiChars('|', '@')) return false;
                if (!buffer.TryWriteDouble(sampleRate)) return false;
            }

            written = buffer.Written;
            return true;
        }
        
    }
}
