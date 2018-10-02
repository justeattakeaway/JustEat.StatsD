using System;

namespace JustEat.StatsD
{
    public interface IStatsDBufferedTransport
    {
        void Send(in ArraySegment<byte> metric);
    }
}
