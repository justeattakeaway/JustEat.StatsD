using System.Globalization;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
	public class WhenIncrementingCounters : BehaviourTest<StatsDMessageFormatter>
	{
		private string _someBucketName;
		private CultureInfo _someCulture;
		private string _result;

		protected override StatsDMessageFormatter CreateSystemUnderTest()
		{
			return new StatsDMessageFormatter();
		}

		protected override void Given()
		{
			_someBucketName = "increment-counter-bucket";
			_someCulture = new CultureInfo("en-US");
		}

		protected override void When()
		{
			_result = SystemUnderTest.Increment(_someBucketName);
		}


		[Then]
		public void FormattedStringShouldBeCorrectlyFormatted()
		{
			_result.ShouldBe(string.Format(_someCulture, "{0}:{1}|c\n", _someBucketName, 1));
		}

		[Then]
		public void NoExceptionsShouldHaveBeenThrown()
		{
			ThrownException.ShouldBe(null);
		}
	}
}
