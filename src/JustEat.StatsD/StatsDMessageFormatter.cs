using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JustEat.StatsD
{
	[Serializable]
	public class StatsDMessageFormatter
	{
		public const string SafeDefaultIsoCultureID = "en-US";

		[ThreadStatic]
		private static Random _random;
		private static Random Random
		{
			get
			{
				return _random ?? (_random = new Random());
			}
		}
		
		private readonly CultureInfo _cultureInfo;
		private const double DefaultSampleRate = 1.0;
		
		public StatsDMessageFormatter() : this(new CultureInfo(SafeDefaultIsoCultureID)) { }

		public StatsDMessageFormatter(CultureInfo ci)
		{
			_cultureInfo = ci;
		}

		public string Timing(long milliseconds, string statBucket)
		{
			return Timing(milliseconds, DefaultSampleRate, statBucket);
		}

		public string Timing(long milliseconds, double sampleRate, string statBucket)
		{
			return Format(sampleRate, string.Format(_cultureInfo, "{0}:{1:d}|ms", statBucket, milliseconds));
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
			var stat = string.Format(_cultureInfo, "{0}:{1}|c", statBucket, magnitude);
			return Format(stat, sampleRate);
		}

		public string Increment(long magnitude, params string[] statBuckets)
		{
			return Increment(magnitude, DefaultSampleRate, statBuckets);
		}

		public string Increment(long magnitude, double sampleRate, params string[] statBuckets)
		{
			return Format(sampleRate, statBuckets.Select(key => string.Format(_cultureInfo, "{0}:{1}|c", key, magnitude)).ToArray());
		}

		public string Gauge(long magnitude, string statBucket)
		{
			var stat = string.Format(_cultureInfo, "{0}:{1}|g", statBucket, magnitude);
			return Format(stat, DefaultSampleRate);
		}

		public string Gauge(long magnitude, string statBucket, DateTime timestamp)
		{
			var stat = string.Format(_cultureInfo, "{0}:{1}|g|@{2}", statBucket, magnitude, timestamp.AsUnixTime());
			return Format(stat, DefaultSampleRate);
		}

		private string Format(String stat, double sampleRate)
		{
			return Format(sampleRate, stat);
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
						formatted.AppendFormat(_cultureInfo, "{0}|@{1:f}", stat, sampleRate);
					}
				}
			}
			else
			{
				foreach (var stat in stats)
				{
					formatted.AppendFormat(_cultureInfo, "{0}", stat);
				}
			}

			return formatted.ToString();
		}
	}
}
