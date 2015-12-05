using System;

namespace JustEat.StatsD
{
    public interface IDisposableTimer : IDisposable
    {
        void Cancel();
        string StatName { get; set; }
    }
}
