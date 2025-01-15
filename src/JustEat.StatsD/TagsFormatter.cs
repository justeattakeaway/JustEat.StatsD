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
    public static IStatsDTagsFormatter CloudWatch => new TrailingTagsFormatter();

    /// <summary>
    /// Gets a DataDog tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter DataDog => new TrailingTagsFormatter();

    /// <summary>
    /// Gets a GraphiteDB tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter GraphiteDb => new GraphiteDbTagsFormatter();

    /// <summary>
    /// Gets an InfluxDB tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter InfluxDb => new InfluxDbTagsFormatter();

    /// <summary>
    /// Gets a Librato tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter Librato => new LibratoTagsFormatter();

    /// <summary>
    /// Gets a SignalFX dimensions formatter.
    /// </summary>
    public static IStatsDTagsFormatter SignalFx => new SignalFxTagsFormatter();

    /// <summary>
    /// Gets a Splunk tags formatter.
    /// </summary>
    public static IStatsDTagsFormatter Splunk => new TrailingTagsFormatter();
}
