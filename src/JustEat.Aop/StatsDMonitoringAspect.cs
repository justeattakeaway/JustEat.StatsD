using System;
using System.Diagnostics;
using System.Globalization;
using PostSharp.Aspects;

namespace JustEat.Aop
{
	[Serializable]
	public class StatsDMonitoringAspect : OnMethodBoundaryAspect
	{
		private readonly string _metricName;
		private StatsDPipe _pipe;

		public StatsDMonitoringAspect(string metricName)
		{
			// TODO: handle figuring out country/tenant prefix
			// TODO: supply the component name
			_metricName = metricName;
		}
		public override bool CompileTimeValidate(System.Reflection.MethodBase method)
		{
			var host = "monitor.je-labs.com";
			var port = 8125;
			_pipe = new StatsDPipe(host, port);
			return true;
		}

		public override void OnEntry(MethodExecutionArgs args)
		{
			args.MethodExecutionTag = Stopwatch.StartNew();
			_pipe.Increment(string.Format(CultureInfo.CurrentCulture, "{0}.attempts", _metricName));
		}

		public override void OnExit(MethodExecutionArgs args)
		{
			_pipe.Increment(string.Format(CultureInfo.CurrentCulture, "{0}.finished", _metricName));
		}

		public override void OnException(MethodExecutionArgs args)
		{
			var sw = (Stopwatch) args.MethodExecutionTag;
			sw.Stop();
			_pipe.Timing(string.Format(CultureInfo.CurrentCulture, "{0}.bad", _metricName), sw.ElapsedMilliseconds);
			_pipe.Increment(string.Format(CultureInfo.CurrentCulture, "{0}.bad", _metricName));
			_pipe.Increment(string.Format(CultureInfo.CurrentCulture, "errors.{0}", args.Exception.GetType().Name));
		}

		public override void OnSuccess(MethodExecutionArgs args)
		{
			var sw = (Stopwatch) args.MethodExecutionTag;
			sw.Stop();
			_pipe.Timing(string.Format(CultureInfo.CurrentCulture, "{0}.good", _metricName), sw.ElapsedMilliseconds);
			_pipe.Increment(string.Format(CultureInfo.CurrentCulture, "{0}.good", _metricName));
		}
	}
}
