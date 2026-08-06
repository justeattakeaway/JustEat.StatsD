using System.Collections.Concurrent;

namespace JustEat.StatsD.Diagnostics;

internal sealed class FakeStatsDPublisher : IStatsDPublisherWithTags
{
    internal sealed record Metric(string Kind, double Value, double SampleRate, string Bucket, Dictionary<string, string?>? Tags)
    {
        /// <summary>
        /// Gets the value published for a counter or a timing, which cannot always be
        /// represented exactly by <see cref="Value"/>, or <see langword="null"/> for a gauge.
        /// </summary>
        internal long? IntegralValue { get; init; }
    }

    internal ConcurrentQueue<Metric> Metrics { get; } = new();

    public void Increment(long value, double sampleRate, string bucket, Dictionary<string, string?>? tags)
        => Metrics.Enqueue(new("counter", value, sampleRate, bucket, tags) { IntegralValue = value });

    public void Gauge(double value, string bucket, Dictionary<string, string?>? tags)
        => Metrics.Enqueue(new("gauge", value, 1, bucket, tags));

    public void Timing(long duration, double sampleRate, string bucket, Dictionary<string, string?>? tags)
        => Metrics.Enqueue(new("timing", duration, sampleRate, bucket, tags) { IntegralValue = duration });
}
