using BenchmarkDotNet.Running;

namespace Benchmark
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            BenchmarkRunner.Run<StatSendingBenchmark>();
            BenchmarkRunner.Run<FormatterBenchmark>();
            BenchmarkRunner.Run<UdpTransportBenchmark>();
        }
    }
}
