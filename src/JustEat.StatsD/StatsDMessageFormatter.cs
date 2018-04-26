using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JustEat.StatsD
{
    public class StatsDMessageFormatter
    {
        private const double DefaultSampleRate = 1.0;

        [ThreadStatic]
        private static Random _random;

        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
        private readonly string _prefix;

        public StatsDMessageFormatter()
            : this(string.Empty) {}

            public StatsDMessageFormatter(string prefix)
        {
            _prefix = prefix;

            if (!string.IsNullOrWhiteSpace(_prefix))
            {
                _prefix = _prefix + "."; // if we have something, then append a . to it to make concatenations easy.
            }
        }

        private static Random Random => _random ?? (_random = new Random());

        public string Timing(long milliseconds, string statBucket)
        {
            return Timing(milliseconds, DefaultSampleRate, statBucket);
        }

        public string Timing(long milliseconds, double sampleRate, string statBucket)
        {
            return Format(sampleRate, string.Format(InvariantCulture, "{0}{1}:{2:d}|ms", _prefix, statBucket, milliseconds));
        }

        public string Decrement(string statBucket)
        {
            return Increment(-1, DefaultSampleRate, statBucket);
        }

        public string Decrement(long magnitude, string statBucket)
        {
            return Decrement(magnitude, DefaultSampleRate, statBucket);
        }


        public string Decrement(long magnitude, double sampleRate, string statBucket)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(magnitude, sampleRate, statBucket);
        }

        public string Decrement(params string[] statBuckets)
        {
            return Increment(-1, DefaultSampleRate, statBuckets);
        }

        public string Decrement(long magnitude, params string[] statBuckets)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(magnitude, DefaultSampleRate, statBuckets);
        }

        public string Decrement(long magnitude, double sampleRate, params string[] statBuckets)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(magnitude, sampleRate, statBuckets);
        }

        public string Increment(string statBucket)
        {
            return Increment(1, DefaultSampleRate, statBucket);
        }

        public string Increment(long magnitude, string statBucket)
        {
            return Increment(magnitude, DefaultSampleRate, statBucket);
        }

        public string Increment(long magnitude, double sampleRate, string statBucket)
        {
            var stat = string.Format(InvariantCulture, "{0}{1}:{2}|c", _prefix, statBucket, magnitude);
            return Format(sampleRate, stat);
        }

        public string Increment(long magnitude, params string[] statBuckets)
        {
            return Increment(magnitude, DefaultSampleRate, statBuckets);
        }

        public string Increment(long magnitude, double sampleRate, params string[] statBuckets)
        {
            return Format(sampleRate, statBuckets.Select(key => string.Format(InvariantCulture, "{0}{1}:{2}|c", _prefix, key, magnitude)).ToArray());
        }

        public string Gauge(double magnitude, string statBucket)
        {
            var stat = string.Format(InvariantCulture, "{0}{1}:{2}|g", _prefix, statBucket, magnitude);
            return Format(DefaultSampleRate, stat);
        }
        public string Gauge(double magnitude, string statBucket, DateTime timestamp)
        {
            var stat = string.Format(InvariantCulture, "{0}{1}:{2}|g|@{3}", _prefix, statBucket, magnitude, timestamp.AsUnixTime());
            return Format(DefaultSampleRate, stat);
        }

        public string Gauge(long magnitude, string statBucket)
        {
            var stat = string.Format(InvariantCulture, "{0}{1}:{2}|g", _prefix, statBucket, magnitude);
            return Format(DefaultSampleRate, stat);
        }

        public string Gauge(long magnitude, string statBucket, DateTime timestamp)
        {
            var stat = string.Format(InvariantCulture, "{0}{1}:{2}|g|@{3}", _prefix, statBucket, magnitude, timestamp.AsUnixTime());
            return Format(DefaultSampleRate, stat);
        }

        public string Event(string name)
        {
            return Increment(name);
        }

        private string Format(double sampleRate, string stat)
        {
            if (sampleRate >= DefaultSampleRate)
            {
                return stat;
            }

            if (Random.NextDouble() <= sampleRate)
            {
                return string.Format(InvariantCulture, "{0}|@{1:f}", stat, sampleRate);
            }

            return string.Empty;
        }

        private string Format(double sampleRate, params string[] stats)
        {
            var formatted = new StringBuilder();
            if (sampleRate < DefaultSampleRate)
            {
                foreach (var stat in stats)
                {
                    if (Random.NextDouble() <= sampleRate)
                    {
                        formatted.AppendFormat(InvariantCulture, "{0}|@{1:f}", stat, sampleRate);
                    }
                }
            }
            else
            {
                foreach (var stat in stats)
                {
                    formatted.Append(stat);
                }
            }

            return formatted.ToString();
        }
    }
}
