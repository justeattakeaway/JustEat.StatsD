using System;
using System.Globalization;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
	public class WhenDecrementingCountersWithAValue : BehaviourTest<StatsDMessageFormatter>
	{
		private string _someBucketName;
		private long _someValueToSend;
		private CultureInfo _someCulture;
		private string _result;

		protected override StatsDMessageFormatter CreateSystemUnderTest()
		{
			return new StatsDMessageFormatter();
		}

		protected override void Given()
		{
			var random = new Random();
			_someBucketName = "counter-bucket";
			_someValueToSend = random.Next(100);
			_someCulture = new CultureInfo("en-US");
		}

		protected override void When()
		{
			_result = SystemUnderTest.Decrement(_someValueToSend, _someBucketName);
		}


		[Then]
		public void FormattedStringShouldBeCorrectlyFormatted()
		{
			_result.ShouldBe(string.Format(_someCulture, "{0}:-{1}|c\n", _someBucketName, _someValueToSend));
		}

		[Then]
		public void NoExceptionsShouldHaveBeenThrown()
		{
			ThrownException.ShouldBe(null);
		}
	}
}
