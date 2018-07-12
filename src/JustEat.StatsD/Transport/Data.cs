using System;

namespace JustEat.StatsD
{
    public ref struct Data
    {
        public Data(byte[] array) : this()
        {
            Array = array ?? throw new ArgumentNullException(nameof(array));
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
