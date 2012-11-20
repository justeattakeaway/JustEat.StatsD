using System;
using System.Globalization;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
	public class WhenFormattingMetricsToGauges : BehaviourTest<StatsDMessageFormatter>
	{
		private string _someBucketName;
		private int _someValueToSend;
		private CultureInfo _someCulture;
		private string _result;

		protected override StatsDMessageFormatter CreateSystemUnderTest()
		{
			return new StatsDMessageFormatter();
		}

		protected override void Given()
		{
			var random = new Random();
			_someBucketName = "gauge-bucket";
			_someValueToSend = random.Next(100);
			_someCulture = new CultureInfo("en-US");
		}

		protected override void When()
		{
			_result = SystemUnderTest.Gauge(_someValueToSend, _someBucketName, _someCulture);
		}


		[Then]
		public void FormattedStringShouldBeCorrectlyFormatted()
		{
			_result.ShouldBe(string.Format(_someCulture, "{0}:{1}|g\n", _someBucketName, _someValueToSend));
		}

		[Then]
		public void NoExceptionsShouldHaveBeenThrown()
		{
			ThrownException.ShouldBe(null);
		}
	}
}
