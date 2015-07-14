using System;
using System.Globalization;
using JustBehave;
using Shouldly;

namespace JustEat.StatsD.Tests
{
    public abstract class WhenTestingGauges : BehaviourTest<StatsDMessageFormatter>
    {
        private string _result;
        private string _someBucketName;
        private CultureInfo _someCulture;
        private long _someValueToSend;

        protected override StatsDMessageFormatter CreateSystemUnderTest()
        {
            return new StatsDMessageFormatter(_someCulture);
        }

        protected override void Given()
        {
            var random = new Random();
            _someBucketName = "gauge-bucket";
            _someValueToSend = random.Next(100);
            _someCulture = new CultureInfo("en-US");
        }

        [Then]
        public void NoExceptionsShouldHaveBeenThrown()
        {
            ThrownException.ShouldBe(null);
        }

        #region Nested type: WhenFormattingAGaugeMetric

        private class WhenFormattingAGaugeMetric : WhenTestingGauges
        {
            protected override void When()
            {
                _result = SystemUnderTest.Gauge(_someValueToSend, _someBucketName);
            }


            [Then]
            public void FormattedStringShouldBeCorrectlyFormatted()
            {
                _result.ShouldBe(string.Format(_someCulture, "{0}:{1}|g", _someBucketName, _someValueToSend));
            }
        }

        #endregion

        #region Nested type: WhenFormattingAGaugeMetricWithABadCulture

        private class WhenFormattingAGaugeMetricWithABadCulture : WhenTestingGauges
        {
            private string _badFormatValueInMetric;

            protected override StatsDMessageFormatter CreateSystemUnderTest()
            {
                return new StatsDMessageFormatter(_someCulture);
            }

            protected override void Given()
            {
                base.Given();
                // Creates a CultureInfo for DK which we know contains bad format for statsd - decimals are comma's.
                _someValueToSend = 100000000;
                var badCulture = new CultureInfo("da-DK");
                // The Metric is in the dk culture i.e, 10000,000 this will fail in statsd, so we pass in a culture when formatting the string.
                _badFormatValueInMetric = _someValueToSend.ToString("0.00", badCulture);
            }


            protected override void When()
            {
                _result = SystemUnderTest.Gauge(_someValueToSend, _someBucketName);
            }


            [Then]
            public void FormattedStringShouldNotContainBadValue()
            {
                _result.ShouldNotContain(_badFormatValueInMetric);
            }

            [Then]
            public void FormattedStringShouldBeCorrectlyFormatted()
            {
                _result.ShouldBe(string.Format(_someCulture, "{0}:{1}|g", _someBucketName, _someValueToSend));
            }
        }

        #endregion

        #region Nested type: WhenFormattingAGaugeMetricWithATimestamp

        private class WhenFormattingAGaugeMetricWithATimestamp : WhenTestingGauges
        {
            private DateTime _timeStamp;

            protected override void Given()
            {
                base.Given();
                _timeStamp = DateTime.Now;
            }

            protected override void When()
            {
                _result = SystemUnderTest.Gauge(_someValueToSend, _someBucketName, _timeStamp);
            }

            [Then]
            public void FormattedStringShouldBeCorrectlyFormatted()
            {
                _result.ShouldBe(string.Format(_someCulture, "{0}:{1}|g|@{2}", _someBucketName, _someValueToSend, _timeStamp.AsUnixTime()));
            }
        }

        #endregion
    }
}
