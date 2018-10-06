using System;

namespace JustEat.StatsD
{
    public interface IStatsDTransport
    {
        void Send(in ArraySegment<byte> metric);
    }
}
