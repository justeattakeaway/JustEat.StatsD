using System;

namespace JustEat.StatsD
{
    public interface IDisposableTimer : IDisposable
    {
        string StatName { get; set; }
    }
}
