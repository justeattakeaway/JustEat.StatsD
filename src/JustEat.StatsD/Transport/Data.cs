using System;

namespace JustEat.StatsD
{
    public static class DataExtensions
    {
        public static ReadOnlySpan<byte> GetSpan(this in Data src)
        {
            if (src.Array == null)
            {
                return src.Span;
            }
            return src.Array.AsSpan();
        }

        public static byte[] GetArray(this in Data src)
        {
            if (src.Array == null)
            {
                return src.Span.ToArray();
            }
            return src.Array;
        }
    }

    public ref struct Data
    {
        public Data(byte[] array) : this()
        {
            Array = array;
        }

        public Data(ReadOnlySpan<byte> span) : this()
        {
            Span = span;
        }

        public readonly byte[] Array;

        public readonly ReadOnlySpan<byte> Span;

        public static implicit operator Data(byte[] source)
        {
            return new Data(source);
        }

        public static implicit operator Data(ReadOnlySpan<byte> source)
        {
            return new Data(source);
        }

        public static implicit operator Data(Span<byte> source)
        {
            return new Data(source);
        }
    }
}
