using System.Collections.Generic;

namespace JustEat.StatsD
{
    public interface IStatsDTransport
    {
        bool Send(string metric);
        bool Send(IEnumerable<string> metrics);
    }
}
