using System;

namespace JustEat.StatsD
{
    public interface IStatsDBufferedTransport
    {
        void Send(ArraySegment<byte> metric);
    }
}
