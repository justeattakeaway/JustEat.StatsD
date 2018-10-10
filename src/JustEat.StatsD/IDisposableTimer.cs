using System;

namespace JustEat.StatsD
{
    /// <summary>
    /// Defines a timer which is disposable.
    /// </summary>
    public interface IDisposableTimer : IDisposable
    {
        /// <summary>
        /// Gets the name of the statsd bucket associated with the timer.
        /// </summary>
        string StatName { get; set; }
    }
}
