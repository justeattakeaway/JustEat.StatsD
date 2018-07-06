using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using RunMode = BenchmarkDotNet.Jobs.RunMode;

namespace Benchmark
{
    public class FastAndDirty : ManualConfig
    {
        public FastAndDirty()
        {
            var job = new Job("", RunMode.Short, InfrastructureMode.InProcess);
            this.Add(job);
            this.Add(DefaultConfig.Instance.GetLoggers().ToArray());
            this.Add(DefaultConfig.Instance.GetColumnProviders().ToArray());
            this.Add(DefaultConfig.Instance.GetAnalysers().ToArray());
            this.Add(DefaultConfig.Instance.GetDiagnosers().ToArray());
            this.Add(new MemoryDiagnoser());
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            //Span<byte> buff = stackalloc byte[128];
            //var formatter = new SpanStatsDMessageFormatter("some.cool.prefix");
            //var writer = new Writer(buff);
            //formatter.Gauge(1000, "some.stat", ref writer);
            //Console.WriteLine(writer.Show());

            //BenchmarkRunner.Run<StatSendingBenchmark>(new FastAndDirty());
            //BenchmarkRunner.Run<FormatterBenchmark>(new FastAndDirty());
            BenchmarkRunner.Run<FormatterBenchmarkSpan>(new FastAndDirty());
        }
    }
}
