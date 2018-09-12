using System;

namespace JustEat.StatsD.Buffered
{
    internal interface IStatsDBufferedTransport
    {
        void Send(ArraySegment<byte> metric);
    }
}
