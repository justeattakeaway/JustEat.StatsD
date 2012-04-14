using System;
using System.Threading;
using JustEat.Testing;
using JustEat.Aop;
using NUnit.Framework;
using Shouldly;

namespace JustEat.Aop.Tests
{
	[Ignore]
	public class WhenIntegratingAgainstARealStatsD : BehaviourTest<StatsDPipe>
	{
		protected override StatsDPipe CreateSystemUnderTest()
		{
			return new StatsDPipe("monitor.je-labs.com", 8125);
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
				SystemUnderTest.Increment("unit-test.bare.orders-count", n);
				Thread.Sleep(TimeSpan.FromMilliseconds(n));
				SystemUnderTest.Timing("unit-test.bare.orders-timing", n);
			}
		}

		[Then]
		public void NoExceptionsShouldHaveBeenThrown()
		{
			ThrownException.ShouldBe(null);
		}
	}
}
