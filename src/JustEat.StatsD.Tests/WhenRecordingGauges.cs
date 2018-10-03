using System;
using System.Globalization;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class WhenRecordingGauges
    {
        [Fact]
        public static void GaugeMetricsAreFormattedCorrectly()
        {
            string statBucket = "gauge-bucket";
            long magnitude = new Random().Next(100);

            var target = new StatsDMessageFormatter();

            string actual = target.Gauge(magnitude, statBucket);

            actual.ShouldBe(string.Format(CultureInfo.InvariantCulture, "{0}:{1}|g", statBucket, magnitude));
        }

        [Fact]
        public static void GaugeMetricsAreFormattedCorrectlyUsingDouble()
        {
            string statBucket = "gauge-bucket";
            //generate a random double value between 0.1 and 100
            double magnitude = new Random().NextDouble() * (100 - 0.1) + 0.1;

            var target = new StatsDMessageFormatter();

            string actual = target.Gauge(magnitude, statBucket);

            actual.ShouldBe(string.Format(CultureInfo.InvariantCulture, "{0}:{1}|g", statBucket, magnitude));
        }
    }
}
