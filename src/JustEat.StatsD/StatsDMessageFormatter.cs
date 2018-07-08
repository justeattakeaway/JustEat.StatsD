using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JustEat.StatsD
{
    internal static class StringBuilderCache
    {
        private const int DefaultCapacity = 256;
        private const int MaxBuilderSize = DefaultCapacity * 3;

        [ThreadStatic]
        private static StringBuilder t_cachedInstance;

        /// <summary>Get a StringBuilder for the specified capacity.</summary>
        /// <remarks>If a StringBuilder of an appropriate size is cached, it will be returned and the cache emptied.</remarks>
        public static StringBuilder Acquire(int capacity = DefaultCapacity)
        {
            if (capacity <= MaxBuilderSize)
            {
                StringBuilder sb = t_cachedInstance;
                if (capacity < DefaultCapacity)
                {
                    capacity = DefaultCapacity;
                }

                if (sb != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= sb.Capacity)
                    {
                        t_cachedInstance = null;
                        sb.Clear();
                        return sb;
                    }
                }
            }
            return new StringBuilder(capacity);
        }

        public static void Release(StringBuilder sb)
        {
            if (sb.Capacity <= MaxBuilderSize)
            {
                t_cachedInstance = sb;
            }
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            string result = sb.ToString();
            Release(sb);
            return result;
        }
    }

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
            var stat = string.Concat(_prefix, statBucket, ":", milliseconds.ToString("d", InvariantCulture), "|ms");
            return Format(sampleRate, stat);
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
            if (magnitude == 1 && sampleRate == 1.0)
            {
                return _prefix + statBucket + ":1|c";
            }

            var stat = string.Concat(_prefix, statBucket, ":", magnitude.ToString(InvariantCulture), "|c");
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

        private static string Format(double sampleRate, string stat)
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

        private static string Format(double sampleRate, params string[] stats)
        {
            var formatted = StringBuilderCache.Acquire();
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
            return StringBuilderCache.GetStringAndRelease(formatted);
        }
    }
}
