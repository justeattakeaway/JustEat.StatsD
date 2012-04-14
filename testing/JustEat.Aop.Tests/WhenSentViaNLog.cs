using System;
using System.Diagnostics;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NUnit.Framework;

namespace JustEat.Aop.Tests
{
	public class WhenSentViaNLog
	{
		[Test]
		public void Firehose()
		{
			var networkTarget = new NLog.Targets.NetworkTarget() {NewLine=true,Address="udp://monitor.je-labs.com:8125",Layout="${message}"};
			SimpleConfigurator.ConfigureForTargetLogging((Target)networkTarget, NLog.LogLevel.Info);
			var logger = LogManager.GetCurrentClassLogger();
			var r = new Random();
			var sw = Stopwatch.StartNew();
			for (var i = 0; i < 1000; i++)
			{
				var n = r.Next(1000);
				logger.Info("nlog-test.foo:1|c\n");
				logger.Info(string.Format("nlog-test.bar:{0}|c\n", n));
				Thread.Sleep(TimeSpan.FromMilliseconds(n));
				logger.Info(string.Format("nlog-test.iteration:{0}|ms\n", n));
			}
			sw.Stop();
			logger.Info(string.Format("nlog-test.whole-test:{0}|ms\n", sw.ElapsedMilliseconds));
		}
	}
}