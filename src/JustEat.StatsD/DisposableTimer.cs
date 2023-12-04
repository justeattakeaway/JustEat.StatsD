using System.Diagnostics;

namespace JustEat.StatsD;

internal sealed class DisposableTimer : IDisposableTimer
{
    private readonly IStatsDPublisher? _publisher;
    private readonly IStatsDPublisherWithTags? _publisherWithTags;
    private readonly Stopwatch _stopwatch;

    private bool _disposed;

    public string Bucket { get; set; }

    public Dictionary<string, string?>? Tags { get; set; }

    public DisposableTimer(IStatsDPublisher publisher, string bucket)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

        if (string.IsNullOrEmpty(bucket))
        {
            throw new ArgumentNullException(nameof(bucket));
        }

        Bucket = bucket;
        _stopwatch = Stopwatch.StartNew();
    }

    public DisposableTimer(IStatsDPublisherWithTags publisher, string bucket, Dictionary<string, string?>? tags)
    {
        _publisherWithTags = publisher ?? throw new ArgumentNullException(nameof(publisher));

        if (string.IsNullOrEmpty(bucket))
        {
            throw new ArgumentNullException(nameof(bucket));
        }

        Bucket = bucket;
        Tags = tags;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _stopwatch.Stop();

            if (string.IsNullOrEmpty(Bucket))
            {
                throw new InvalidOperationException($"The {nameof(Bucket)} property must have a value.");
            }

            if (_publisherWithTags is not null)
            {
                _publisherWithTags.Timing(_stopwatch.Elapsed, Bucket, Tags);
            }
            else
            {
                _publisher!.Timing(_stopwatch.Elapsed, Bucket);
            }
        }
    }
}
