using System;
using System.Globalization;
using System.Text;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public class WhenRecordingCounters
    {
        private readonly string _statBucket;
        private readonly string[] _statBuckets;
        private readonly long _value;
        private readonly CultureInfo _culture;

        public WhenRecordingCounters()
        {
            _statBucket = "counter-bucket";
            _statBuckets = new[] { "counter-bucket-1", "counter-bucket-2" };

            _value = new Random().Next(1, 100);
            _culture = new CultureInfo("en-US");
        }

        [Fact]
        public void DecrementingCounterIsCorrect()
        {
            // Arrange
            var target = new StatsDMessageFormatter(_culture);

            // Act
            string actual = target.Decrement(_statBucket);

            // Assert
            actual.ShouldBe(string.Format(_culture, "{0}:-{1}|c", _statBucket, 1));
        }

        [Fact]
        public void DecrementingCounterWithValueIsCorrect()
        {
            // Arrange
            var target = new StatsDMessageFormatter(_culture);

            // Act
            string actual = target.Decrement(_value, _statBucket);

            // Assert
            actual.ShouldBe(string.Format(_culture, "{0}:-{1}|c", _statBucket, _value));
        }

        [Fact]
        public void DecrementingMultipleCountersIsCorrect()
        {
            // Arrange
            var target = new StatsDMessageFormatter(_culture);

            // Act
            string actual = target.Decrement(_value, _statBuckets);

            // Assert
            var expected = new StringBuilder();

            foreach (var stat in _statBuckets)
            {
                expected.AppendFormat(_culture, "{0}:-{1}|c", stat, _value);
            }

            actual.ShouldBe(expected.ToString());
        }

        [Fact]
        public void IncrementingCounterIsCorrect()
        {
            // Arrange
            var target = new StatsDMessageFormatter(_culture);

            // Act
            string actual = target.Increment(_statBucket);

            // Assert
            actual.ShouldBe(string.Format(_culture, "{0}:{1}|c", _statBucket, 1));
        }

        [Fact]
        public void IncrementingCounterWithValueIsCorrect()
        {
            // Arrange
            var target = new StatsDMessageFormatter(_culture);

            // Act
            string actual = target.Increment(_value, _statBucket);

            // Assert
            actual.ShouldBe(string.Format(_culture, "{0}:{1}|c", _statBucket, _value));
        }

        [Fact]
        public void IncrementingMultipleCountersIsCorrect()
        {
            // Arrange
            var target = new StatsDMessageFormatter(_culture);

            // Act
            string actual = target.Increment(_value, _statBuckets);

            // Assert
            var expected = new StringBuilder();

            foreach (var stat in _statBuckets)
            {
                expected.AppendFormat(_culture, "{0}:{1}|c", stat, _value);
            }

            actual.ShouldBe(expected.ToString());
        }

        [Fact]
        public void RaisingEventIsCorrect()
        {
            // Arrange
            var target = new StatsDMessageFormatter(_culture);

            // Act
            string actual = target.Event("foo");

            // Assert
            actual.ShouldEndWith("|c");
        }

        [Fact]
        public void GaugeIsCorrect()
        {
            // Arrange
            var target = new StatsDMessageFormatter(_culture);

            // Act
            string actual = target.Gauge(_value, _statBucket);

            // Assert
            actual.ShouldBe(string.Format(_culture, "{0}:{1}|g", _statBucket, _value));
        }

        [Fact]
        public void TimingIsCorrect()
        {
            // Arrange
            var target = new StatsDMessageFormatter(_culture);

            // Act
            string actual = target.Timing(_value, _statBucket);

            // Assert
            actual.ShouldBe(string.Format(_culture, "{0}:{1}|ms", _statBucket, _value));
        }

        [Fact]
        public void ValuesArePrefixed()
        {
            var prefix = "foo";
            var target = new StatsDMessageFormatter(_culture, prefix);

            var actualGauge = target.Gauge(_value, _statBucket);
            var actualDecrement = target.Decrement(_statBucket);
            var actualIncrement = target.Increment(_statBucket);
            var actualTiming = target.Timing(_value, _statBucket);

            actualGauge.ShouldStartWith($"{prefix}.");
            actualDecrement.ShouldStartWith($"{prefix}.");
            actualIncrement.ShouldStartWith($"{prefix}.");
            actualTiming.ShouldStartWith($"{prefix}.");
        }
    }
}
