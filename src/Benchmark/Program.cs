using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JustEat.StatsD;
using JustEat.StatsD.V2;

namespace Benchmark
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args);
        }
    }

    [MemoryDiagnoser]
    public class Utf8FormatterBench
    {
        private static readonly StatsDUtf8Formatter Formatter = new StatsDUtf8Formatter("hello.world");
        private static readonly byte[] Buffer = new byte[512];

        private static readonly StatsDMessageFormatter FormatterOld = new StatsDMessageFormatter("hello.world");

        [Benchmark(Baseline = true)]
        public void Original()
        {
            Encoding.UTF8.GetBytes(FormatterOld.Gauge(255.1, "some.neat.bucket"));
        }

        [Benchmark]
        public void HeapBuffer()
        {
            Formatter.TryFormat(StatsDMessage.Gauge(255.1, "some.neat.bucket"), 1, Buffer, out var written);
        }

        [Benchmark]
        public void StackBuffer()
        {
            var statsDMessage = StatsDMessage.Counter(1, "some.neat.bucket");
            Span<byte> buffer = stackalloc byte[Formatter.GetBufferSize(statsDMessage)];
            Formatter.TryFormat(statsDMessage, 1, buffer, out var written);
        }
    }
}
