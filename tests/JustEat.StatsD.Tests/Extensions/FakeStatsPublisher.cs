namespace JustEat.StatsD.Extensions;

public class FakeStatsPublisher : IStatsDPublisher, IStatsDPublisherWithTags
{
    public FakeStatsPublisher()
    {
        BucketNames = new List<string>();
    }

    public int CallCount { get; set; }

    public int DisposeCount { get; set; }

    public TimeSpan LastDuration { get; set; }

    public List<string> BucketNames { get; }

    public void Dispose()
    {
        DisposeCount++;
    }

    public void Increment(long value, double sampleRate, string bucket)
    {
        CallCount++;
        BucketNames.Add(bucket);
    }

    public void Gauge(double value, string bucket)
    {
        CallCount++;
        BucketNames.Add(bucket);
    }

    public void Timing(long duration, double sampleRate, string bucket)
    {
        CallCount++;
        LastDuration = TimeSpan.FromMilliseconds(duration);
        BucketNames.Add(bucket);
    }

    public void Increment(long value, double sampleRate, string bucket, Dictionary<string, string?>? tags)
    {
        CallCount++;
        BucketNames.Add(bucket);
    }

    public void Gauge(double value, string bucket, Dictionary<string, string?>? tags)
    {
        CallCount++;
        BucketNames.Add(bucket);
    }

    public void Timing(long duration, double sampleRate, string bucket, Dictionary<string, string?>? tags)
    {
        CallCount++;
        LastDuration = TimeSpan.FromMilliseconds(duration);
        BucketNames.Add(bucket);
    }
}
