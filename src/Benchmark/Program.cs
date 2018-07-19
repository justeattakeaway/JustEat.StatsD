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

    internal static class Program
    {
        internal static void Main(string[] args)
        {
            BenchmarkRunner.Run<StatSendingBenchmark>();
            BenchmarkRunner.Run<FormatterBenchmark>();
            BenchmarkRunner.Run<UdpTransportBenchmark>();
            BenchmarkRunner.Run<UdpStatSendingBenchmark>();
        }
    }
}
