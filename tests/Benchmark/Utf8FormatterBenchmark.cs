using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.Buffered.Tags;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class Utf8FormatterBenchmark
    {
        private static readonly StatsDUtf8Formatter FormatterBuffer = new StatsDUtf8Formatter("hello.world", new NoOpTagsFormatter());

        private static readonly byte[] Buffer = new byte[512];

        [Benchmark]
        public void BufferBased()
        {
            var tags = new Dictionary<string, string?>
            {
                ["key"] = "value",
                ["key2"] = "value2",
            };

            FormatterBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", null), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", null), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", null), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", tags), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", tags), 1, Buffer, out _);
            FormatterBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", tags), 1, Buffer, out _);
        }
    }
}
