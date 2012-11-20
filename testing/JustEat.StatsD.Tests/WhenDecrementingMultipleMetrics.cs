using System;
using System.Globalization;
using System.Text;
using JustEat.Testing;
using Shouldly;

namespace JustEat.StatsD.Tests
{
	public class WhenDecrementingMultipleMetrics : BehaviourTest<StatsDMessageFormatter>
	{
		private string[] _someBucketName;
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
			_someBucketName = new string [] {"counter-bucket-1","counter-bucket-2"};
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
			var expectedString = new StringBuilder();

			foreach (var stat in _someBucketName)
			{
				expectedString.AppendFormat(_someCulture, "{0}:-{1}|c\n", stat, _someValueToSend);
			}
			_result.ShouldBe(expectedString.ToString());
		}

		[Then]
		public void NoExceptionsShouldHaveBeenThrown()
		{
			ThrownException.ShouldBe(null);
		}
	}
}
