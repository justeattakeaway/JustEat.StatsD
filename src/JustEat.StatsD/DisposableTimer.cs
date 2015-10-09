using System;
using System.Diagnostics;

namespace JustEat.StatsD
{
    public class DisposableTimer : IDisposable
    {
        private bool _disposed;
        private IStatsDPublisher _publisher;
        private Stopwatch _stopwatch;
        private readonly string _bucket;

        public DisposableTimer(IStatsDPublisher publisher, string bucket)
        {
            if (publisher == null)
            {
                throw new ArgumentNullException("publisher");
            }

            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException("bucket");
            }

            _publisher = publisher;
            _bucket = bucket;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _stopwatch.Stop();

                _publisher.Timing(_stopwatch.Elapsed, _bucket);
                
                _stopwatch = null;
                _publisher = null;
            }
        }
    }
}
