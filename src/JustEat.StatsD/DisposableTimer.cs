using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JustEat.StatsD
{
    internal sealed class DisposableTimer : IDisposableTimer
    {
        private readonly IStatsDPublisher _publisher;
        private readonly Stopwatch _stopwatch;

        private bool _disposed;

        public string Bucket { get; set; }

        public Dictionary<string, string?>? Tags { get; set; }

        public DisposableTimer(IStatsDPublisher publisher, string bucket)
            : this(publisher, bucket, null)
        {
        }

        public DisposableTimer(IStatsDPublisher publisher, string bucket, Dictionary<string, string?>? tags)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

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

                _publisher.Timing(_stopwatch.Elapsed, Bucket, Tags);
            }
        }
    }
}
