using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JustEat.StatsD
{
    [Serializable]
    public class StatsDMessageFormatter
    {
        private readonly Random _random = new Random();

        public string Timing(long milliseconds, string statBucket)
        {
            return Timing(milliseconds, 1.0, statBucket);
        }

        public string Timing(long milliseconds, double sampleRate, string statBucket)
        {
			return Timing(milliseconds,sampleRate,statBucket,CultureInfo.CurrentCulture);
        }

		public string Timing(long milliseconds, double sampleRate, string statBucketn, CultureInfo culture)
		{
			return Format(culture, sampleRate, string.Format(culture, "{0}:{1:d}|ms", statBucketn, milliseconds));
		}

        public string Decrement(string statBucket)
        {
            return Increment(-1, 1.0, statBucket);
        }

        public string Decrement(long magnitude, string statBucket)
        {
            return Decrement(magnitude, 1.0, statBucket);
        }

        public string Decrement(long magnitude, double sampleRate, string statBucket)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(magnitude, sampleRate, statBucket);
        }

        public string Decrement(params string[] statBuckets)
        {
            return Increment(-1, 1.0, statBuckets);
        }

        public string Decrement(long magnitude, params string[] statBuckets)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(magnitude, 1.0, statBuckets);
        }

        public string Decrement(long magnitude, double sampleRate, params string[] statBuckets)
        {
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(magnitude, sampleRate, statBuckets);
        }

        public string Increment(string statBucket)
        {
            return Increment(1, 1.0, statBucket);
        }

        public string Increment(long magnitude, string statBucket)
        {
            return Increment(magnitude, 1.0, statBucket);
        }

        public string Increment(long magnitude, double sampleRate, string statBucket)
        {
        	return Increment(magnitude, sampleRate, statBucket, CultureInfo.CurrentCulture);
        }

		public string Increment(long magnitude, double sampleRate, string statBucket,  CultureInfo culture)
		{
			var stat = string.Format(culture, "{0}:{1}|c", statBucket, magnitude);
			return Format(stat, sampleRate);
		}

        public string Increment(long magnitude, double sampleRate, params string[] statBuckets)
        {
			return Format(CultureInfo.CurrentCulture, sampleRate, statBuckets.Select(key => string.Format(CultureInfo.CurrentCulture, "{0}:{1}|c", key, magnitude)).ToArray());
        }

        public string Gauge(long magnitude, string statBucket)
        {
			return Gauge(magnitude, statBucket, CultureInfo.CurrentCulture);
        }

		public string Gauge(long magnitude, string statBucket, CultureInfo cultureInfo)
		{
			var stat = string.Format(cultureInfo, "{0}:{1}|g", statBucket, magnitude);
			return Format(stat, 1.0);
		}
		
		public string Gauge(long magnitude, string statBucket, CultureInfo cultureInfo, DateTime timeStamp)
		{
			var stat = string.Format(cultureInfo, "{0}:{1}|g|@{2}", statBucket, magnitude, timeStamp.AsUnixTime());
			return Format(stat, 1.0);
		}

        private string Format(String stat, double sampleRate)
        {
            return Format(CultureInfo.CurrentCulture, sampleRate, stat);
        }

        private string Format(IFormatProvider cultureInfo, double sampleRate, params string[] stats)
        {
            var formatted = new StringBuilder();
            if (sampleRate < 1.0)
            {
                foreach (var stat in stats)
                {
                    if (_random.NextDouble() <= sampleRate)
                    {
						formatted.AppendFormat(cultureInfo, "{0}|@{1:f}\n", stat, sampleRate);
                    }
                }
            }
            else
            {
                foreach (var stat in stats)
                {
					formatted.AppendFormat(cultureInfo, "{0}\n", stat);
                }
            }

            return formatted.ToString();
        }
    }
}
