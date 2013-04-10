using System;

namespace JustEat.StatsD
{
	public interface IStatsDPublisher : IDisposable
	{
		void Increment(string bucket);
		void Increment(long value, string bucket);
		void Increment(long value, double sampleRate, string bucket);
		void Increment(long value, double sampleRate, params string[] buckets);
		void Decrement(string bucket);
		void Decrement(long value, string bucket);
		void Decrement(long value, double sampleRate, string bucket);
		void Decrement(long value, double sampleRate, params string[] buckets);
		void Gauge(long value, string bucket);
		void Gauge(long value, string bucket, DateTime timeStamp);
		void Timing(TimeSpan duration, string bucket);
		void Timing(TimeSpan duration, double sampleRate, string bucket);
	    void Event(string name);
	    void Event(string name, DateTime when);
	}
}
