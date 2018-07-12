using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Benchmark
{
    public class FastRunOnMultipleRuntimes : ManualConfig
    {
        public FastRunOnMultipleRuntimes(Job basis)
        {
            Add(basis.With(Runtime.Core).With(Jit.RyuJit).WithGcServer(true).With(CsProjCoreToolchain.NetCoreApp21));
            Add(basis.With(Runtime.Core).With(Jit.RyuJit).WithGcServer(true).With(CsProjCoreToolchain.NetCoreApp20));
            Add(basis.With(Runtime.Clr).With(Jit.RyuJit).WithGcServer(true));

            Add(DefaultConfig.Instance);
            Add(MemoryDiagnoser.Default);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<StatSendingBenchmark>(new FastRunOnMultipleRuntimes(Job.MediumRun));
            BenchmarkRunner.Run<FormatterBenchmark>(new FastRunOnMultipleRuntimes(Job.MediumRun));
        }
    }
}
