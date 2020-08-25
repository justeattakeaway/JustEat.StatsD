using System;

namespace JustEat.StatsD.Buffered
{
    /// <summary>
    /// A class representing the buffer where the StatsD message is written.
    /// </summary>
    public ref struct Buffer
    {
        internal Buffer(Span<byte> source)
        {
            Tail = source;
            Written = 0;
        }

        internal Span<byte> Tail;
        internal int Written;
    }
}
