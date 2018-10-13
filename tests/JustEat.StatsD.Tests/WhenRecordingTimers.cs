using System;
using System.Globalization;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class WhenRecordingTimers
    {
        [Fact]
        public static void TimingMetricsAreFormattedCorrectly()
        {
            string statBucket = "timing-bucket";
            long milliseconds = new Random().Next(1000);

            var target = new StatsDMessageFormatter();

            string actual = target.Timing(milliseconds, statBucket);

            actual.ShouldBe(string.Format(CultureInfo.InvariantCulture, "{0}:{1:d}|ms", statBucket, milliseconds));
        }
    }
}
