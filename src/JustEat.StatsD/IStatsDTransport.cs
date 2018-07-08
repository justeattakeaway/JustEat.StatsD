using System;

namespace JustEat.StatsD
{
    public interface IStatsDTransport
    {
        void Send(ReadOnlySpan<byte> metric);
    }
}
