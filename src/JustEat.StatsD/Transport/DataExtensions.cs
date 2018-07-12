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
}
