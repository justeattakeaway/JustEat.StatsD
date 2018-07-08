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
            //BenchmarkRunner.Run<StatSendingBenchmark>(new FastAndDirty());
            //BenchmarkRunner.Run<FormatterBenchmark>(new FastAndDirty());
            BenchmarkRunner.Run<StatSendingBenchmark>(new FastAndDirty());
        }
    }
}
