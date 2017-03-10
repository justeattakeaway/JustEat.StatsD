using System;
using Shouldly;

namespace JustEat.StatsD.Extensions
{
    public static class PublisherAssertions
    {
        public static void SingleStatNameIs(FakeStatsPublisher publisher, string statName)
        {
            publisher.CallCount.ShouldBe(1);
            publisher.DisposeCount.ShouldBe(0);

            publisher.BucketNames.Count.ShouldBe(1);
            publisher.BucketNames[0].ShouldBe(statName);
        }

        public static void LastDurationIs(FakeStatsPublisher publisher, int expectedMillis)
        {
            DurationIsMoreOrLess(publisher.LastDuration, TimeSpan.FromMilliseconds(expectedMillis));
        }

        private static readonly TimeSpan deltaMoreOrLess = TimeSpan.FromMilliseconds(150);

        private static void DurationIsMoreOrLess(TimeSpan actual, TimeSpan expected)
        {
            var expectedLower = expected.Subtract(deltaMoreOrLess);
            var expectedUpper = expected.Add(deltaMoreOrLess);

            actual.ShouldBeGreaterThanOrEqualTo(expectedLower);
            actual.ShouldBeLessThanOrEqualTo(expectedUpper);
        }
    }
}
