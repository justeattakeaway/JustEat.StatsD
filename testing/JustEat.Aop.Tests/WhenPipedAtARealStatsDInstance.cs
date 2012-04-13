using System.Globalization;
using NUnit.Framework;
using Shouldly;

namespace JustEat.Aop.Tests
{
	public class WhenPipedAtARealStatsDInstance : BehaviourTest<StatsDPipe>
	{
		protected override void CreateSystemUnderTest()
		{
			SystemUnderTest = new StatsDPipe("monitor.je-labs.com", 8125);
		}

		protected override void Given()
		{

		}

		protected override void When()
		{
			SystemUnderTest.Increment(string.Format(CultureInfo.CurrentCulture, "unit-test.{0}", GetType().Name));
		}

		[Then]
		public void AnIncrementShouldNotFailToSend()
		{
			ThrownException.ShouldBeNull();
		}
	}
}
