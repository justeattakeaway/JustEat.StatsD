using System;

namespace JustEat.StatsD.Tests.Extensions
{
    class FakePublisher : IStatsDPublisher
    {
        public int CallCount { get; set; }

        public void Dispose()
        {
        }

        public void Increment(string bucket)
        {
            CallCount++;
        }

        public void Increment(long value, string bucket)
        {
            CallCount++;
        }

        public void Increment(long value, double sampleRate, string bucket)
        {
            CallCount++;
        }

        public void Increment(long value, double sampleRate, params string[] buckets)
        {
            CallCount++;
        }

        public void Decrement(string bucket)
        {
            CallCount++;
        }

        public void Decrement(long value, string bucket)
        {
            CallCount++;
        }

        public void Decrement(long value, double sampleRate, string bucket)
        {
            CallCount++;
        }

        public void Decrement(long value, double sampleRate, params string[] buckets)
        {
            CallCount++;
        }

        public void Gauge(long value, string bucket)
        {
            CallCount++;
        }

        public void Gauge(long value, string bucket, DateTime timestamp)
        {
            CallCount++;
        }

        public void Timing(TimeSpan duration, string bucket)
        {
            CallCount++;
        }

        public void Timing(TimeSpan duration, double sampleRate, string bucket)
        {
            CallCount++;
        }

        public void MarkEvent(string name)
        {
            CallCount++;
        }
    }
}
