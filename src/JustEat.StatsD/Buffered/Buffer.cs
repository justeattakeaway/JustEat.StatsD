using System;

namespace JustEat.StatsD.Buffered
{
    internal ref struct Buffer<T>
    {
        public Buffer(Span<T> source)
        {
            Tail = source;
            Written = 0;
        }

        public Span<T> Tail;
        public int Written;
    }
}
