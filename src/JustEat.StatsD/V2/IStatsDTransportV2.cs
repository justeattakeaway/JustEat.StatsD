using System;

namespace JustEat.StatsD.V2
{
    public interface IStatsDTransportV2
    {
        void Send(ArraySegment<byte> metric);
    }
}
