using System;
using NUnit.Framework;

namespace JustEat.StatsD.Tests.Extensions
{
    public static class PublisherAssertions
    {
        public static void SingleStatNameIs(FakeStatsPublisher publisher, string statName)
        {
            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));

            Assert.That(publisher.BucketNames.Count, Is.EqualTo(1));
            Assert.That(publisher.BucketNames[0], Is.EqualTo(statName));
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

            Assert.That(actual, Is.GreaterThanOrEqualTo(expectedLower));
            Assert.That(actual, Is.LessThanOrEqualTo(expectedUpper));
        }
    }
}
