using JustEat.StatsD.TagsFormatters;

namespace JustEat.StatsD;

/// <summary>
/// A class that can be used to retrieve supported <see cref="IStatsDTagsFormatter"/> implementations for some of the major metric providers.
/// </summary>
public static class TagsFormatter
{
    /// <summary>
    /// Gets an AWS CloudWatch tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter CloudWatch => TrailingTagsFormatter.Instance;

    /// <summary>
    /// Gets a DataDog tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter DataDog => TrailingTagsFormatter.Instance;

    /// <summary>
    /// Gets a GraphiteDB tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter GraphiteDb { get; } = new GraphiteDbTagsFormatter();

    /// <summary>
    /// Gets an InfluxDB tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter InfluxDb { get; } = new InfluxDbTagsFormatter();

    /// <summary>
    /// Gets a Librato tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter Librato { get; } = new LibratoTagsFormatter();

    /// <summary>
    /// Gets a SignalFX dimensions formatter.
    /// </summary>
    public static IStatsDTagsFormatter SignalFx { get; } = new SignalFxTagsFormatter();

    /// <summary>
    /// Gets a Splunk tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter Splunk => TrailingTagsFormatter.Instance;
}
