using System;
using System.Collections.Generic;

namespace JustEat.StatsD.Tests.Extensions
{
    class FakeStatsPublisher : IStatsDPublisher
    {
        public int CallCount { get; set; }
        public int DisposeCount { get; set; }
        public TimeSpan LastDuration { get; set; }

        public List<string> BucketNames { get; private set; }

        public FakeStatsPublisher()
        {
            BucketNames = new List<string>();
        }

        public void Dispose()
        {
            DisposeCount++;
        }

        public void Increment(string bucket)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Increment(long value, string bucket)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Increment(long value, double sampleRate, string bucket)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Increment(long value, double sampleRate, params string[] buckets)
        {
            CallCount++;
            BucketNames.AddRange(buckets);
        }

        public void Decrement(string bucket)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Decrement(long value, string bucket)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Decrement(long value, double sampleRate, string bucket)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Decrement(long value, double sampleRate, params string[] buckets)
        {
            CallCount++;
            BucketNames.AddRange(buckets);
        }

        public void Gauge(long value, string bucket)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Gauge(long value, string bucket, DateTime timestamp)
        {
            CallCount++;
            BucketNames.Add(bucket);
        }

        public void Timing(TimeSpan duration, string bucket)
        {
            CallCount++;
            LastDuration = duration;
            BucketNames.Add(bucket);
        }

        public void Timing(TimeSpan duration, double sampleRate, string bucket)
        {
            CallCount++;
            LastDuration = duration;
            BucketNames.Add(bucket);
        }

        public void MarkEvent(string name)
        {
            CallCount++;
            BucketNames.Add(name);
        }
    }
}
