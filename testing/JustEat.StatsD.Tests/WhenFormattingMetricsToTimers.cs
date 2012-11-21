using System;
using System.Globalization;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
    public class WhenFormattingMetricsToTimers : BehaviourTest<StatsDMessageFormatter>
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
        	_someBucketName = "timing-bucket";
            _someValueToSend = random.Next(1000);
			_someCulture = new CultureInfo("en-US");
        }

        protected override void When()
        {
			_result = SystemUnderTest.Timing(_someValueToSend,_someBucketName);
        }


		[Then]
		public void FormattedStringShouldBeCorrectlyFormatted()
		{
			_result.ShouldBe(string.Format(_someCulture, "{0}:{1:d}|ms\n", _someBucketName, _someValueToSend));
		}

        [Then]
        public void NoExceptionsShouldHaveBeenThrown()
        {
            ThrownException.ShouldBe(null);
        }
    }
}
