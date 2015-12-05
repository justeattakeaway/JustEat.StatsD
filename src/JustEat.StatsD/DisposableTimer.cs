using System;
using System.Diagnostics;

namespace JustEat.StatsD
{
    internal class DisposableTimer : IDisposableTimer
    {
        private bool _disposed;

        private IStatsDPublisher _publisher;
        private Stopwatch _stopwatch;

        public string StatName { get; set; }

        public DisposableTimer(IStatsDPublisher publisher, string statName)
        {
            if (publisher == null)
            {
                throw new ArgumentNullException("publisher");
            }

            _publisher = publisher;
            StatName = statName;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _stopwatch.Stop();

                if (!string.IsNullOrEmpty(StatName))
                {
                    _publisher.Timing(_stopwatch.Elapsed, StatName);
                }

                _stopwatch = null;
                _publisher = null;
            }
        }

        public void Cancel()
        {
            StatName = null;
        }
    }
}
