using System;
using Shouldly;

namespace JustEat.StatsD.Tests.Extensions
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

        private static void DurationIsMoreOrLess(TimeSpan expected, TimeSpan actual)
        {
            TimeSpan delta = TimeSpan.FromMilliseconds(100);

            var expectedLower = expected.Subtract(delta);
            var expectedUpper = expected.Add(delta);

            actual.ShouldBeGreaterThanOrEqualTo(expectedLower);
            actual.ShouldBeLessThanOrEqualTo(expectedUpper);
        }
    }
}
