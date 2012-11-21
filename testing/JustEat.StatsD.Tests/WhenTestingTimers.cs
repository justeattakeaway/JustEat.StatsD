using System;
using System.Globalization;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
    public abstract class WhenTestingTimers : BehaviourTest<StatsDMessageFormatter>
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

		public class WhenFormattingATimingMetric : WhenTestingTimers
		{
			protected override void When()
			{
				_result = SystemUnderTest.Timing(_someValueToSend, _someBucketName);
			}
			
			[Then]
			public void FormattedStringShouldBeCorrectlyFormatted()
			{
				_result.ShouldBe(string.Format(_someCulture, "{0}:{1:d}|ms\n", _someBucketName, _someValueToSend));
			}
		}
		public class WhenAddingASampleRateToATiming : WhenTestingTimers
		{
			private double _sampleRate;

			protected override void Given()
			{
				base.Given();
				_sampleRate = 0.9;
			}

			protected override void When()
			{
				// Need to mock the results... so random isnt so random here...otherwise we get a test fail when comparing the formatted string...
				SystemUnderTest.Timing(_someValueToSend, _sampleRate, _someBucketName);
			}

			// Not running this test till i add mocking over this object and introduce an interface bla bla bla.
			//[Then]
			//public void FormattedStringShouldBeCorrectlyFormatted()
			//{
			//    _result.ShouldBe(string.Format(_someCulture, "{0}:{1}|ms|@{2:f}\n", _someBucketName, _someValueToSend, _sampleRate));
			//}
		}

        [Then]
        public void NoExceptionsShouldHaveBeenThrown()
        {
            ThrownException.ShouldBe(null);
        }
    }
}
