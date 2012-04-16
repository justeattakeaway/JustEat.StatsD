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
	[Ignore]
	[TestFixture]
	public class WhenSentViaNLog
	{
		[Test]
		public void Firehose()
		{
			LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config", false) {AutoReload=true};
			var logger = LogManager.GetLogger("Firehose-StatsD");
			var r = new Random();
			var sw = Stopwatch.StartNew();
			var metric = string.Format("nlog-config-test.{0:HH-mm-ss}", DateTime.UtcNow);
			for (var i = 0; i < 1000; i++)
			{
				var n = r.Next(1000);
				Trace.Write(string.Format("trace: {0}", n));
				Debug.Write(string.Format("debug: {0}", n));
				Console.WriteLine(string.Format("console: {0}", n));
				logger.Info("{0}.foo:1|c\n", metric);
				logger.Info(string.Format("{0}.bar:{1}|c\n", metric, n));
				Thread.Sleep(TimeSpan.FromMilliseconds(n));
				logger.Info(string.Format("{0}.iteration:{1}|ms\n", metric, n));
			}
			sw.Stop();
			logger.Info(string.Format("{0}.whole-test:{1}|ms\n", metric, sw.ElapsedMilliseconds));
		}
	}
}