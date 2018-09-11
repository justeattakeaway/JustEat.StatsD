using System;

namespace JustEat.StatsD.V2
{
    internal interface IStatsDTransportV2
    {
        void Send(ArraySegment<byte> metric);
    }
}
