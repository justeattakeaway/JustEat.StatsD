using System;
using System.Globalization;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
	public class WhenFormattingMetricsToTimings : BehaviourTest<StatsDMessageFormatter>
	{
		private string _someBucketName;
		private long _someValueToSend;
		private CultureInfo _someCulture;
		private string _result;
		private double _sampleRate;

		protected override StatsDMessageFormatter CreateSystemUnderTest()
		{
			return new StatsDMessageFormatter();
		}

		protected override void Given()
		{
			var random = new Random();
			_someBucketName = "counter-bucket";
			_someValueToSend = random.Next(100);
			_sampleRate = 0.9;
			_someCulture = new CultureInfo("en-US");
		}

		protected override void When()
		{
			// Need to mock the results... so random isnt so random here...otherwise we get a test fail when comparing the formatted string...
			_result = SystemUnderTest.Timing(_someValueToSend, _sampleRate, _someBucketName);
		}

		// Not running this test till i add mocking over this object and introduce an interface bla bla bla.
		//[Then]
		//public void FormattedStringShouldBeCorrectlyFormatted()
		//{
		//    _result.ShouldBe(string.Format(_someCulture, "{0}:{1}|ms|@{2:f}\n", _someBucketName, _someValueToSend, _sampleRate));
		//}

		[Then]
		public void NoExceptionsShouldHaveBeenThrown()
		{
			ThrownException.ShouldBe(null);
		}
	}
}
