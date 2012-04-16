using System.Globalization;
using JustEat.Aop;
using JustEat.Testing;
using Rhino.Mocks;
using Shouldly;

namespace JustEat.Aop.Tests
{
	public class WhenMetricsHappen : BehaviourTest<StatsDMessageFormatter>
	{
		private string _metricName;
		private string _increment;

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
