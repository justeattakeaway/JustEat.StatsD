using System;

namespace JustEat.StatsD
{
    /// <summary>
    /// Defines a timer which is disposable.
    /// </summary>
    public interface IDisposableTimer : IDisposable
    {
        /// <summary>
        /// Gets or sets the name of the StatsD bucket associated with the timer.
        /// </summary>
        string StatName { get; set; }
    }
}
