using System.Collections.Concurrent;

namespace JustEat.StatsD.Diagnostics;

internal sealed class FakeStatsDPublisher : IStatsDPublisherWithTags
{
    internal sealed record Metric(string Kind, double Value, double SampleRate, string Bucket, Dictionary<string, string?>? Tags);

    internal ConcurrentQueue<Metric> Metrics { get; } = new();

    public void Increment(long value, double sampleRate, string bucket, Dictionary<string, string?>? tags)
        => Metrics.Enqueue(new("counter", value, sampleRate, bucket, tags));

    public void Gauge(double value, string bucket, Dictionary<string, string?>? tags)
        => Metrics.Enqueue(new("gauge", value, 1, bucket, tags));

    public void Timing(long duration, double sampleRate, string bucket, Dictionary<string, string?>? tags)
        => Metrics.Enqueue(new("timing", duration, sampleRate, bucket, tags));
}
