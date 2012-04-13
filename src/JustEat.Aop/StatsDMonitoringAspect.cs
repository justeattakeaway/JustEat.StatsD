using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using PostSharp.Aspects;

namespace JustEat.Aop
{
	[Serializable]
	public class StatsDMonitoringAspect : OnMethodBoundaryAspect
	{
		private readonly string _metricName;
		private readonly StatsDPipe _pipe;

		public StatsDMonitoringAspect(string metricName)
		{
			// TODO: handle figuring out country/tenant prefix
			// TODO: supply the component name
			_metricName = metricName;
			// TODO: better way of dealing with configuration?  Pass in a Configuration instance instead...?
			var host = ConfigurationManager.AppSettings["StatsDHostName"];
			var port = Convert.ToInt32(ConfigurationManager.AppSettings["StatsDPort"]);
			_pipe = new StatsDPipe(host, port);
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
