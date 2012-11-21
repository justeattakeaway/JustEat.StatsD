using System;
using System.Globalization;
using System.Text;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
	public abstract class WhenTestingCounters : BehaviourTest<StatsDMessageFormatter>
	{
		private string _someBucketName;
		private CultureInfo _someCulture;
		private string _result;
		private long _someValueToSend;

		protected override StatsDMessageFormatter CreateSystemUnderTest()
		{
			return new StatsDMessageFormatter(_someCulture);
		}

		protected override void Given()
		{
			var random = new Random();
			_someValueToSend = random.Next(100);
			_someBucketName = "counter-bucket";
			_someCulture = new CultureInfo("en-US");
		}

		public class WhenIncrementingCounters : WhenTestingCounters
		{
			protected override void When()
			{
				_result = SystemUnderTest.Increment(_someBucketName);
			}


			[Then]
			public void FormattedStringShouldBeCorrectlyFormatted()
			{
				_result.ShouldBe(string.Format(_someCulture, "{0}:{1}|c\n", _someBucketName, 1));
			}
		}

		public class WhenDecrementingCounters : WhenTestingCounters
		{
			protected override void When()
			{
				_result = SystemUnderTest.Decrement(_someBucketName);
			}


			[Then]
			public void FormattedStringShouldBeCorrectlyFormatted()
			{
				_result.ShouldBe(string.Format(_someCulture, "{0}:-{1}|c\n", _someBucketName, 1));
			}
		}

		public class WhenIncrementingCountersWithAValue : WhenTestingCounters
		{
			protected override void When()
			{
				_result = SystemUnderTest.Increment(_someValueToSend, _someBucketName);
			}

			[Then]
			public void FormattedStringShouldBeCorrectlyFormatted()
			{
				_result.ShouldBe(string.Format(_someCulture, "{0}:{1}|c\n", _someBucketName, _someValueToSend));
			}
		}

		public class WhenDecrementingCountersWithAValue : WhenTestingCounters
		{
			protected override void When()
			{
				_result = SystemUnderTest.Decrement(_someValueToSend, _someBucketName);
			}

			[Then]
			public void FormattedStringShouldBeCorrectlyFormatted()
			{
				_result.ShouldBe(string.Format(_someCulture, "{0}:-{1}|c\n", _someBucketName, _someValueToSend));
			}
		}

		public class WhenAddingASampleRateToACounter : WhenTestingCounters
		{
			private double _sampleRate;

			protected override void Given()
			{
				base.Given();
				_sampleRate = 0.9;
			}

			protected override void When()
			{
				SystemUnderTest.Increment(_someValueToSend, _sampleRate, _someBucketName);
			}

			// Again, this test is too flaky for now...
			//[Then]
			//public void FormattedStringShouldBeCorrectlyFormatted()
			//{
			//    _result.ShouldBe(string.Format(_someCulture, "{0}:{1}|c|@{2:f}\n", _someBucketName, _someValueToSend, _sampleRate));
			//}
		}

		public class WhenIncrementingMultipleMetrics : WhenTestingCounters
		{
			private new string[] _someBucketName;

			protected override void Given()
			{
				base.Given();
				_someBucketName = new string[] { "counter-bucket-1", "counter-bucket-2" };
			}

			protected override void When()
			{
				_result = SystemUnderTest.Increment(_someValueToSend, _someBucketName);
			}

			[Then]
			public void FormattedStringShouldBeCorrectlyFormatted()
			{
				var expectedString = new StringBuilder();

				foreach (var stat in _someBucketName)
				{
					expectedString.AppendFormat(_someCulture, "{0}:{1}|c\n", stat, _someValueToSend);
				}

				_result.ShouldBe(expectedString.ToString());
			}
		}

		public class WhenDecrementingMultipleMetrics : WhenTestingCounters
		{
			private new string[] _someBucketName;

			protected override void Given()
			{
				base.Given();
				_someBucketName = new string[] { "counter-bucket-1", "counter-bucket-2" };
			}

			protected override void When()
			{
				_result = SystemUnderTest.Decrement(_someValueToSend, _someBucketName);
			}

			[Then]
			public void FormattedStringShouldBeCorrectlyFormatted()
			{
				var expectedString = new StringBuilder();

				foreach (var stat in _someBucketName)
				{
					expectedString.AppendFormat(_someCulture, "{0}:-{1}|c\n", stat, _someValueToSend);
				}

				_result.ShouldBe(expectedString.ToString());
			}
		}

		[Then]
		public void NoExceptionsShouldHaveBeenThrown()
		{
			ThrownException.ShouldBe(null);
		}
	}
}
