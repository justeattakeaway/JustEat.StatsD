using System.Collections.Generic;

namespace JustEat.StatsD
{
    public interface IStatsDTransport
    {
        void Send(string metric);
        void Send(IEnumerable<string> metrics);
    }
}
