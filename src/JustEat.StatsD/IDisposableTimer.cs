namespace JustEat.StatsD
{
    /// <summary>
    /// Defines a timer which is disposable.
    /// </summary>
    public interface IDisposableTimer : IDisposable
    {
        /// <summary>
        /// Gets or sets the StatsD bucket associated with the timer.
        /// </summary>
        string Bucket { get; set; }
    }
}
