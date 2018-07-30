using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Benchmark
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args, new MultipleRuntimes(Job.ShortRun));
        }

        private class MultipleRuntimes : ManualConfig
        {
            public MultipleRuntimes(Job basis)
            {
                Add(basis.With(Runtime.Core).With(Jit.RyuJit).WithGcServer(true).With(CsProjCoreToolchain.NetCoreApp21));
                Add(basis.With(Runtime.Core).With(Jit.RyuJit).WithGcServer(true).With(CsProjCoreToolchain.NetCoreApp20));

                Add(DefaultConfig.Instance);
                Add(MemoryDiagnoser.Default);
            }
        }
    }
}
