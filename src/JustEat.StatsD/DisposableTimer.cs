using System;
using System.Diagnostics;

namespace JustEat.StatsD
{
    internal sealed class DisposableTimer : IDisposableTimer
    {
        private bool _disposed;

        private IStatsDPublisher _publisher;
        private Stopwatch _stopwatch;

        public string Bucket { get; set; }

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

                _publisher.Timing(_stopwatch.Elapsed, Bucket);

                _stopwatch = null;
                _publisher = null;
            }
        }
    }
}
