using System;

namespace JustEat.StatsD.Buffered
{
    public interface IStatsDBufferedTransport
    {
        void Send(ArraySegment<byte> metric);
    }
}
