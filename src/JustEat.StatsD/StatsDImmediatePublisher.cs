using System;
using System.Globalization;

namespace JustEat.StatsD
{
	/// <summary>
	///     Will synchronously publish stats at statsd as you make calls; will not batch sends.
	/// </summary>
	public class StatsDImmediatePublisher : IStatsDPublisher
	{
		private readonly StatsDMessageFormatter _formatter;
		private readonly IStatsDUdpClient _transport;

		public StatsDImmediatePublisher(CultureInfo cultureInfo, string hostNameOrAddress, int port)
		{
			_formatter = new StatsDMessageFormatter(cultureInfo);
			_transport = new StatsDUdpClient(hostNameOrAddress, port);
		}

		public StatsDImmediatePublisher(string hostNameOrAddress, int port) : this(new CultureInfo(StatsDMessageFormatter.SafeDefaultIsoCultureID), hostNameOrAddress, port) {}

		public void Increment(string bucket)
		{
			_transport.Send(_formatter.Increment(bucket));
		}

		public void Increment(long value, string bucket)
		{
			_transport.Send(_formatter.Increment(value, bucket));
		}

		public void Increment(long value, double sampleRate, string bucket)
		{
			_transport.Send(_formatter.Increment(value, sampleRate, bucket));
		}

		public void Increment(long value, double sampleRate, params string[] buckets)
		{
			_transport.Send(_formatter.Increment(value, sampleRate, buckets));
		}

		public void Decrement(string bucket)
		{
			_transport.Send(_formatter.Decrement(bucket));
		}

		public void Decrement(long value, string bucket)
		{
			_transport.Send(_formatter.Decrement(value, bucket));
		}

		public void Decrement(long value, double sampleRate, string bucket)
		{
			_transport.Send(_formatter.Decrement(value, sampleRate, bucket));
		}

		public void Decrement(long value, double sampleRate, params string[] buckets)
		{
			_transport.Send(_formatter.Decrement(value, sampleRate, buckets));
		}

		public void Gauge(long value, string bucket)
		{
			_transport.Send(_formatter.Gauge(value, bucket));
		}

		public void Gauge(long value, string bucket, DateTime timeStamp)
		{
			_transport.Send(_formatter.Gauge(value, bucket, timeStamp));
		}

		public void Timing(TimeSpan duration, string bucket)
		{
			_transport.Send(_formatter.Timing(Convert.ToInt64(duration.TotalMilliseconds), bucket));
		}

		public void Timing(TimeSpan duration, double sampleRate, string bucket)
		{
			_transport.Send(_formatter.Timing(Convert.ToInt64(duration.TotalMilliseconds), sampleRate, bucket));
		}
	}
}
