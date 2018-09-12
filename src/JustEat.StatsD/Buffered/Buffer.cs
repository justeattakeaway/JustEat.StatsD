using System;

namespace JustEat.StatsD.Buffered
{
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
}
