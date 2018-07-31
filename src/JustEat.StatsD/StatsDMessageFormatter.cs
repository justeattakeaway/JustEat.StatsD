using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JustEat.StatsD
{
    internal static class StringBuilderCache
    {
        private const int DefaultCapacity = 128;
        private const int MaxBuilderSize = DefaultCapacity * 3;

        [ThreadStatic]
        private static StringBuilder t_cachedInstance;

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

        public static string GetStringAndRelease(this StringBuilder sb)
        {
            string result = sb.ToString();
            Release(sb);
            return result;
        }

        public static StringBuilder AppendDouble(this StringBuilder sb, double value)
        {
#if !NETCOREAPP2_1
            return sb.Append(value.ToString(CultureInfo.InvariantCulture));
#else
            return sb.Append(value);
#endif
        }

        public static StringBuilder AppendLong(this StringBuilder sb, long value)
        {
#if !NETCOREAPP2_1
            return sb.Append(value.ToString(CultureInfo.InvariantCulture));
#else
            return sb.Append(value);
#endif
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
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                _prefix = prefix + "."; // if we have something, then append a . to it to make concatenations easy.
            }
            else
            {
                _prefix = prefix;
            }
        }

        private static Random Random => _random ?? (_random = new Random());

        public string Timing(long milliseconds, string statBucket)
        {
            return Timing(milliseconds, DefaultSampleRate, statBucket);
        }

        public string Timing(long milliseconds, double sampleRate, string statBucket)
        {
            var stat = StringBuilderCache.Acquire()
                .Append(_prefix)
                .Append(statBucket)
                .Append(':')
                .AppendLong(milliseconds)
                .Append("|ms");
            
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

            var stat = StringBuilderCache.Acquire()
                .Append(_prefix)
                .Append(statBucket)
                .Append(":")
                .AppendLong(magnitude)
                .Append("|c");

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
            var stat = StringBuilderCache.Acquire()
                .Append(_prefix)
                .Append(statBucket)
                .Append(':')
                .AppendDouble(magnitude)
                .Append("|g");

            return Format(DefaultSampleRate, stat);
        }

        public string Gauge(double magnitude, string statBucket, DateTime timestamp)
        {
            var stat = StringBuilderCache.Acquire()
                .Append(_prefix)
                .Append(statBucket)
                .Append(':')
                .AppendDouble(magnitude)
                .Append("|g|@")
                .AppendDouble(timestamp.AsUnixTime());

            return Format(DefaultSampleRate, stat);
        }

        public string Gauge(long magnitude, string statBucket)
        {
            var stat = StringBuilderCache.Acquire()
                .Append(_prefix)
                .Append(statBucket)
                .Append(':')
                .AppendLong(magnitude)
                .Append("|g");

            return Format(DefaultSampleRate, stat);
        }

        public string Gauge(long magnitude, string statBucket, DateTime timestamp)
        {
            var stat = StringBuilderCache.Acquire()
                .Append(_prefix)
                .Append(statBucket)
                .Append(':')
                .AppendLong(magnitude)
                .Append("|g|@")
                .AppendDouble(timestamp.AsUnixTime());

            return Format(DefaultSampleRate, stat);
        }

        public string Event(string name)
        {
            return Increment(name);
        }

        private static string Format(double sampleRate, StringBuilder stat)
        {
            if (sampleRate >= DefaultSampleRate)
            {
                return stat.GetStringAndRelease();
            }

            if (Random.NextDouble() <= sampleRate)
            {
                return stat.AppendFormat(InvariantCulture, "|@{0:f}", sampleRate).GetStringAndRelease();
            }

            StringBuilderCache.Release(stat);
            return string.Empty;
        }

        private string Format(double sampleRate, params string[] stats)
        {
            var formatted = StringBuilderCache.Acquire(stats.Length * 128);
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

            return formatted.GetStringAndRelease();
        }
    }
}
