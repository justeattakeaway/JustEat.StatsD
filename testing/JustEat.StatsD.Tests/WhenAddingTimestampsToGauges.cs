using System;
using System.Globalization;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
	public class WhenAddingTimestampsToGauges : BehaviourTest<StatsDMessageFormatter>
	{
		private string _someBucketName;
		private int _someValueToSend;
		private CultureInfo _someCulture;
		private DateTime _timeStamp;
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
			_timeStamp = DateTime.Now;
		}

		protected override void When()
		{
			_result = SystemUnderTest.Gauge(_someValueToSend, _someBucketName, _someCulture, _timeStamp);
		}


		[Then]
		public void FormattedStringShouldBeCorrectlyFormatted()
		{
			_result.ShouldBe(string.Format(_someCulture, "{0}:{1}|g|@{2}\n", _someBucketName, _someValueToSend, _timeStamp.AsUnixTime()));
		}

		[Then]
		public void NoExceptionsShouldHaveBeenThrown()
		{
			ThrownException.ShouldBe(null);
		}
	}
}
