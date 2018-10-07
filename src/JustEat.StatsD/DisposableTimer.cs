using System;
using System.Diagnostics;

namespace JustEat.StatsD
{
    internal sealed class DisposableTimer : IDisposableTimer
    {
        private bool _disposed;

        private IStatsDPublisher _publisher;
        private Stopwatch _stopwatch;

        public string StatName { get; set; }

        public DisposableTimer(IStatsDPublisher publisher, string statName)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

            if (string.IsNullOrEmpty(statName))
            {
                throw new ArgumentNullException(nameof(statName));
            }

            StatName = statName;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _stopwatch.Stop();

                if (string.IsNullOrEmpty(StatName))
                {
                    throw new InvalidOperationException("StatName must be set");
                }

                _publisher.Timing(_stopwatch.Elapsed, StatName);

                _stopwatch = null;
                _publisher = null;
            }
        }
    }
}
