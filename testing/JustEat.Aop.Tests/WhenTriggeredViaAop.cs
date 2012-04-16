using System;
using System.Threading;
using JustEat.Testing;
using JustEat.Aop;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Shouldly;

namespace JustEat.Aop.Tests
{
	public class MonitoredViaAop
	{
		private readonly Random _random = new Random();
		
		[StatsDMonitoringAspect("unit-test.aop")]
		public void InterestingMethod()
		{
			Thread.Sleep(TimeSpan.FromMilliseconds(_random.Next(1000)));
			if (_random.Next(10) > 8)
			{
				throw new Exception();
			}
		}
	}

	public class WhenTriggeredViaAop : BehaviourTest<MonitoredViaAop>
	{
		private Logger _log;
		private MemoryTarget _memoryTarget;

		protected override void Given()
		{
			NLog.LogLevel minLevel = NLog.LogLevel.Info;
			_memoryTarget = new MemoryTarget();
			_memoryTarget.Layout = (Layout)"${message}";
			SimpleConfigurator.ConfigureForTargetLogging((Target)_memoryTarget, minLevel);
			_log = LogManager.GetCurrentClassLogger();
		}

		protected override void When()
		{
			var random = new Random();
			for (var i = 0; i < 1000; i++)
			{
				try
				{
					SystemUnderTest.InterestingMethod();
				}
				catch (Exception ex)
				{
					var foo = ex.Message;
					// intentionally swallow; the aspect should log some errors
				}
			}
		}

		[Then]
		public void NoExceptionsShouldBeThrown()
		{
			ThrownException.ShouldBe(null);
		}

		[Then]
		public void LoggingShouldHaveOccurred()
		{
			_memoryTarget.Logs.Count.ShouldBeGreaterThan(0);
		}
	}
}
