using System;
using System.Collections.Generic;

namespace JustEat.StatsD
{
    public interface IStatsDUdpClient
    {
        bool Send(string metric);
        bool Send(IEnumerable<string> metrics);
    }
}
