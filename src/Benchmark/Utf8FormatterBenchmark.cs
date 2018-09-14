using System.Text;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.Buffered;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class Utf8FormatterBenchmark
    {
        private static readonly StatsDMessageFormatter FormatterString = new StatsDMessageFormatter("hello.world");
        private static readonly StatsDUtf8Formatter FormatterBuffer = new StatsDUtf8Formatter("hello.world");

        private static readonly byte[] Buffer = new byte[512];

        [Benchmark(Baseline = true)]
        public void StringBased()
        {
            Encoding.UTF8.GetBytes(FormatterString.Gauge(255, "some.neat.bucket"));
            Encoding.UTF8.GetBytes(FormatterString.Timing(255, "some.neat.bucket"));
            Encoding.UTF8.GetBytes(FormatterString.Increment(255, "some.neat.bucket"));
        }

        [Benchmark]
        public void BufferBased()
        {
            FormatterBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket"), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket"), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket"), 1, Buffer, out _);
        }
    }
}
