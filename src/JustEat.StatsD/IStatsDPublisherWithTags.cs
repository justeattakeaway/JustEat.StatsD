namespace JustEat.StatsD;

/// <summary>
/// Defines an interface for publishing counters, gauges and timers to StatsD with tags.
/// </summary>
public interface IStatsDPublisherWithTags
{
    /// <summary>
    /// Publishes a counter for the specified bucket and value.
    /// </summary>
    /// <param name="value">The value to increment the counter by.</param>
    /// <param name="sampleRate">The sample rate for the counter.</param>
    /// <param name="bucket">The bucket to increment the counter for.</param>
    /// <param name="tags">The tag(s) to publish with the counter.</param>
    void Increment(long value, double sampleRate, string bucket, Dictionary<string, string?>? tags);

    /// <summary>
    /// Publishes a gauge for the specified bucket and value.
    /// </summary>
    /// <param name="value">The value to publish for the gauge.</param>
    /// <param name="bucket">The bucket to publish the gauge for.</param>
    /// <param name="tags">The tag(s) to publish with the gauge.</param>
    void Gauge(double value, string bucket, Dictionary<string, string?>? tags);

    /// <summary>
    /// Publishes a timer for the specified bucket and value.
    /// </summary>
    /// <param name="duration">The value to publish for the timer.</param>
    /// <param name="sampleRate">The sample rate for the timer.</param>
    /// <param name="bucket">The bucket to publish the timer for.</param>
    /// <param name="tags">The tag(s) to publish with the timer.</param>
    void Timing(long duration, double sampleRate, string bucket, Dictionary<string, string?>? tags);
}
