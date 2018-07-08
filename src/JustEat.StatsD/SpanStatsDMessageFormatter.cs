using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

namespace JustEat.StatsD
{
    public static class FixedBufferExtensions
    {
        public static string Show(in this FixedBuffer src) => Encoding.UTF8.GetString(src.Get());

        public static Span<byte> Get(in this FixedBuffer src) => src.Buffer.Slice(0, src.Position);

        public static ref FixedBuffer Add(ref this FixedBuffer src, byte[] str)
        {
            str.CopyTo(src.Buffer.Slice(src.Position));
            src.Position += str.Length;
            return ref src;
        }

        public static ref FixedBuffer Add(ref this FixedBuffer src, ReadOnlySpan<byte> str)
        {
            str.CopyTo(src.Buffer.Slice(src.Position));
            src.Position += str.Length;
            return ref src;
        }

        public static ref FixedBuffer Add(ref this FixedBuffer src, byte str)
        {
            src.Buffer[src.Position] = str;
            src.Position += 1;
            return ref src;
        }

        public static ref FixedBuffer Add(ref this FixedBuffer src, string str)
        {
            var written = Encoding.UTF8.GetBytes(str, src.Buffer.Slice(src.Position));
            src.Position += written;
            return ref src;
        }

        public static ref FixedBuffer Add(ref this FixedBuffer src, double value) 
        {
            if (!Utf8Formatter.TryFormat(value, src.Buffer.Slice(src.Position), out int written, new StandardFormat('f', 2)))
            {
                throw new OutOfMemoryException();
            }

            src.Position += written;
            return ref src;
        }

        public static ref FixedBuffer Add(ref this FixedBuffer src, long value)
        {
            if (!Utf8Formatter.TryFormat(value, src.Buffer.Slice(src.Position), out int written, new StandardFormat('d')))
            {
                throw new OutOfMemoryException();
            }

            src.Position += written;
            return ref src;
        }

        public static void Clear(ref this FixedBuffer src)
        {
            src.Position = 0;
        }
    }

    public ref struct FixedBuffer
    {
        public Span<byte> Buffer;
        public int Position;

        public FixedBuffer(Span<byte> buffer) : this()
        {
            Buffer = buffer;
            Position = 0;
        }
    }

    public class SpanStatsDMessageFormatter
    {
        private const double DefaultSampleRate = 1.0;

        [ThreadStatic]
        private static Random _random;

        private readonly byte[] _prefix;

        public SpanStatsDMessageFormatter()
            : this(string.Empty) {}

        public SpanStatsDMessageFormatter(string prefix)
        {
            _prefix = !string.IsNullOrWhiteSpace(prefix) ?
                Encoding.UTF8.GetBytes(prefix + ".") :
                Array.Empty<byte>();
        }

        private static Random Random => _random ?? (_random = new Random());

        public void Timing(long milliseconds, string statBucket, ref FixedBuffer fixedBuffer)
        {
            Timing(milliseconds, DefaultSampleRate, statBucket, ref fixedBuffer);
        }

        private static readonly byte[] TimingSuffix = Encoding.UTF8.GetBytes("|ms");

        public void Timing(long milliseconds, double sampleRate, string statBucket, ref FixedBuffer fixedBuffer)
        {
            fixedBuffer.Add(_prefix).Add(statBucket).Add(Colon).Add(milliseconds).Add(TimingSuffix);

            Format(sampleRate, ref fixedBuffer);
        }

        public void Decrement(string statBucket, ref FixedBuffer fixedBuffer)
        {
           Increment(-1, DefaultSampleRate, statBucket, ref fixedBuffer);
        }

        public void Decrement(long magnitude, string statBucket, ref FixedBuffer fixedBuffer)
        {
            Decrement(magnitude, DefaultSampleRate, statBucket, ref fixedBuffer);
        }

        public void Decrement(long magnitude, double sampleRate, string statBucket, ref FixedBuffer fixedBuffer)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            Increment(magnitude, sampleRate, statBucket, ref fixedBuffer);
        }

        public void Increment(string statBucket, ref FixedBuffer fixedBuffer)
        {
            Increment(1, DefaultSampleRate, statBucket, ref fixedBuffer);
        }

        public void Increment(long magnitude, string statBucket, ref FixedBuffer fixedBuffer)
        {
            Increment(magnitude, DefaultSampleRate, statBucket, ref fixedBuffer);
        }

        private const byte Colon = (byte) ':';
        private static readonly byte[] IncrementColonBar1C = Encoding.UTF8.GetBytes(":1|c");
        private static readonly byte[] IncrementBarC = Encoding.UTF8.GetBytes("|c");

        public void Increment(long magnitude, double sampleRate, string statBucket, ref FixedBuffer fixedBuffer)
        {
            if (magnitude == 1 && sampleRate == 1.0)
            {
                fixedBuffer.Add(_prefix).Add(statBucket).Add(IncrementColonBar1C);
                return;
            }

            fixedBuffer.Add(_prefix).Add(statBucket).Add(Colon).Add(magnitude).Add(IncrementBarC);

            Format(sampleRate, ref fixedBuffer);
        }
        
        private static readonly byte[] GaugeSuffix = Encoding.UTF8.GetBytes("|g");
        private static readonly byte[] GaugeSuffixExtra = Encoding.UTF8.GetBytes("|g|@");

        public void Gauge(double magnitude, string statBucket, ref FixedBuffer fixedBuffer)
        {
            fixedBuffer
                .Add(_prefix)
                .Add(statBucket)
                .Add(Colon)
                .Add(magnitude)
                .Add(GaugeSuffix);

            Format(DefaultSampleRate, ref fixedBuffer);
        }

        public void Gauge(double magnitude, string statBucket, DateTime timestamp, ref FixedBuffer fixedBuffer)
        {
            fixedBuffer
                .Add(_prefix)
                .Add(statBucket)
                .Add(Colon)
                .Add(magnitude)
                .Add(GaugeSuffixExtra)
                .Add(timestamp.AsUnixTime());

            Format(DefaultSampleRate, ref fixedBuffer);
        }

        public void Gauge(long magnitude, string statBucket, ref FixedBuffer fixedBuffer)
        {
            fixedBuffer
                 .Add(_prefix)
                 .Add(statBucket)
                 .Add(Colon)
                 .Add(magnitude)
                 .Add(GaugeSuffix);

            Format(DefaultSampleRate, ref fixedBuffer);
        }

        public void Gauge(long magnitude, string statBucket, DateTime timestamp, ref FixedBuffer fixedBuffer)
        {
            fixedBuffer
                .Add(_prefix)
                .Add(statBucket)
                .Add(Colon)
                .Add(magnitude)
                .Add(GaugeSuffixExtra)
                .Add(timestamp.AsUnixTime());

            Format(DefaultSampleRate, ref fixedBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Event(string name, ref FixedBuffer fixedBuffer)
        {
            Increment(name, ref fixedBuffer);
        }

        private const byte Bar = (byte) '|';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Format(double sampleRate, ref FixedBuffer stat)
        {
            if (sampleRate >= DefaultSampleRate)
                return;

            if (Random.NextDouble() <= sampleRate)
            {
                stat.Add(Bar).Add(sampleRate);
            }
            else
                stat.Clear();
        }
    }
}
