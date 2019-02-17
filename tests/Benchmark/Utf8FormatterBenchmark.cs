using BenchmarkDotNet.Attributes;
using JustEat.StatsD.Buffered;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class Utf8FormatterBenchmark
    {
        private static readonly StatsDUtf8Formatter FormatterBuffer = new StatsDUtf8Formatter("hello.world");

        private static readonly byte[] Buffer = new byte[512];

        [Benchmark]
        public void BufferBased()
        {
            FormatterBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket"), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket"), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket"), 1, Buffer, out _);
        }
    }
}
