using System;
using System.Threading;
using JustEat.Testing;
using JustEat.Aop;
using NUnit.Framework;
using Shouldly;

namespace JustEat.Aop.Tests
{
	[Ignore]
	public class WhenIntegratingAgainstARealStatsD : BehaviourTest<StatsDMessageFormatter>
	{
		protected override StatsDMessageFormatter CreateSystemUnderTest()
		{
			return new StatsDMessageFormatter();
		}

		protected override void Given()
		{
			
		}

		protected override void When()
		{
			var random = new Random();
			for (var i = 0; i < 1000; i++)
			{
				var n = random.Next(100);
				SystemUnderTest.Increment("unit-test.bare.orders-inc");
				SystemUnderTest.Increment(n, "unit-test.bare.orders-count");
				Thread.Sleep(TimeSpan.FromMilliseconds(n));
				SystemUnderTest.Timing(n, "unit-test.bare.orders-timing");
			}
		}

		[Then]
		public void NoExceptionsShouldHaveBeenThrown()
		{
			ThrownException.ShouldBe(null);
		}
	}
}
