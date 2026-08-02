namespace JustEat.StatsD.Diagnostics;

/// <summary>
/// An enumeration of the kinds of StatsD metric an instrument's measurements are published as.
/// </summary>
internal enum StatsDMetricKind
{
    /// <summary>
    /// The measurements are deltas published as a StatsD counter.
    /// </summary>
    Counter,

    /// <summary>
    /// The measurements are cumulative totals converted to deltas and published as a StatsD counter.
    /// </summary>
    CumulativeCounter,

    /// <summary>
    /// The measurements are published as a StatsD gauge.
    /// </summary>
    Gauge,

    /// <summary>
    /// The measurements are published as a StatsD timer.
    /// </summary>
    Timing,
}
