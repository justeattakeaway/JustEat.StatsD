using System.Diagnostics.Metrics;

namespace JustEat.StatsD.Diagnostics;

/// <summary>
/// A class representing the options to use for publishing <see cref="System.Diagnostics.Metrics"/> measurements to StatsD.
/// </summary>
public sealed class StatsDMetricsOptions
{
    /// <summary>
    /// Gets or sets the interval at which observable instruments are polled for measurements.
    /// </summary>
    /// <remarks>
    /// The default value is 10 seconds.
    /// </remarks>
    public TimeSpan ObservableInstrumentsPollingInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets a value indicating whether histogram measurements recorded in seconds
    /// (instruments whose unit is <c>s</c>) are converted to milliseconds before being
    /// published as StatsD timers.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/>. OpenTelemetry semantic conventions record
    /// durations in seconds (for example the built-in ASP.NET Core <c>http.server.request.duration</c>
    /// histogram), whereas StatsD timers are conventionally measured in milliseconds.
    /// </remarks>
    public bool ConvertSecondsToMilliseconds { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional delegate to use to generate the StatsD bucket name for an instrument.
    /// </summary>
    /// <remarks>
    /// If <see langword="null"/>, the bucket name is <c>{meter-name}.{instrument-name}</c>.
    /// </remarks>
    public Func<Instrument, string>? BucketNameProvider { get; set; }

    /// <summary>
    /// Gets or sets an optional delegate to use to determine the sample rate for an instrument.
    /// </summary>
    /// <remarks>
    /// If <see langword="null"/>, all measurements are published.
    /// </remarks>
    public Func<Instrument, double>? SampleRateProvider { get; set; }
}
