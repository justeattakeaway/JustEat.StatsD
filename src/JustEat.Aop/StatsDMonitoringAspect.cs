using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using NLog;
using PostSharp.Aspects;

namespace JustEat.Aop
{
	[Serializable]
	public sealed class StatsDMonitoringAspect : OnMethodBoundaryAspect
	{
		private readonly string _metricName;
		private readonly StatsDMessageFormatter _statsD;

		public StatsDMonitoringAspect(string metricName)
		{
			// TODO: handle figuring out country/tenant prefix
			// TODO: supply the component name
			_metricName = metricName;
			_statsD = new StatsDMessageFormatter();
		}

		public override void OnEntry(MethodExecutionArgs args)
		{
			var loggerName = string.Format("{0}-StatsD", args.Method.DeclaringType.FullName);
			Console.WriteLine(loggerName);
			var logger = LogManager.GetLogger(loggerName);
			args.MethodExecutionTag = new Dictionary<string, object>
			{
				{ "Stopwatch", Stopwatch.StartNew() },
				{"Logger", logger}
			};
			logger.Trace(_statsD.Increment(string.Format(CultureInfo.CurrentCulture, "{0}.attempts", _metricName)));
		}

		public override void OnExit(MethodExecutionArgs args)
		{
			var logger = (Logger)((IDictionary<string,object>)args.MethodExecutionTag)["Logger"];
			logger.Trace(_statsD.Increment(string.Format(CultureInfo.CurrentCulture, "{0}.finished", _metricName)));
		}

		public override void OnException(MethodExecutionArgs args)
		{
			var bag = (IDictionary<string, object>) args.MethodExecutionTag;
			var sw = (Stopwatch)bag["Stopwatch"];
			sw.Stop();
			var logger = (Logger)bag["Logger"];
			logger.Trace(_statsD.Timing(sw.ElapsedMilliseconds, string.Format(CultureInfo.CurrentCulture, "{0}.bad", _metricName)));
			logger.Trace(_statsD.Increment(string.Format(CultureInfo.CurrentCulture, "{0}.bad", _metricName)));
			logger.Trace(_statsD.Increment(string.Format(CultureInfo.CurrentCulture, "errors.{0}", args.Exception.GetType().Name)));
		}

		public override void OnSuccess(MethodExecutionArgs args)
		{
			var bag = (IDictionary<string, object>)args.MethodExecutionTag;
			var logger = (Logger)bag["Logger"];
			var sw = (Stopwatch)bag["Stopwatch"];
			sw.Stop();
			logger.Trace(_statsD.Timing(sw.ElapsedMilliseconds, string.Format(CultureInfo.CurrentCulture, "{0}.good", _metricName)));
			logger.Trace(_statsD.Increment(string.Format(CultureInfo.CurrentCulture, "{0}.good", _metricName)));
		}
	}
}
