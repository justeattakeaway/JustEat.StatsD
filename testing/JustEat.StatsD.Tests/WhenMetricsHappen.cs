using System.Globalization;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
    public class WhenMetricsHappen : BehaviourTest<StatsDMessageFormatter>
    {
        private string _increment;
        private string _metricName;

        protected override StatsDMessageFormatter CreateSystemUnderTest()
        {
            return new StatsDMessageFormatter();
        }

        protected override void Given()
        {
            _metricName = string.Format(CultureInfo.CurrentCulture, "unit-test.{0}", GetType().Name);
        }

        protected override void When()
        {
            _increment = SystemUnderTest.Increment(_metricName);
        }

        [Then]
        public void NoExceptionsShouldBeThrown()
        {
            ThrownException.ShouldBe(null);
        }

        [Then]
        public void Increment1ShouldBeCorrectlyFormatted()
        {
            //_increment.ShouldMatch("unit-test.")
        }
    }
}
