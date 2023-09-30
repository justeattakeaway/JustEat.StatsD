using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using JustEat.StatsD;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.TagsFormatters;

namespace Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net50)]
[SimpleJob(RuntimeMoniker.Net60)]
public class Utf8FormatterBenchmark
{
    private static readonly StatsDUtf8Formatter FormatterBuffer = new("hello.world", new NoOpTagsFormatter());
    private static readonly StatsDUtf8Formatter FormatterWithTagsBuffer = new("hello.world", TagsFormatter.DataDog);

    private static readonly Dictionary<string, string?> EmptyTags = new();
    private static readonly Dictionary<string, string?> AnyValidTags = new()
    {
        ["key"] = "value",
        ["key2"] = "value2",
    };

    private static readonly byte[] Buffer = new byte[512];

    [Benchmark]
    public void BufferBased()
    {
        FormatterBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", null), 1, Buffer, out _);
        FormatterBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", null), 1, Buffer, out _);
        FormatterBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", null), 1, Buffer, out _);
    }

    [Benchmark]
    public void BufferBasedIgnoringTags()
    {
        FormatterBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", AnyValidTags), 1, Buffer, out _);
        FormatterBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", AnyValidTags), 1, Buffer, out _);
        FormatterBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", AnyValidTags), 1, Buffer, out _);
    }

    [Benchmark]
    public void BufferBasedIgnoringEmptyTags()
    {
        FormatterBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", EmptyTags), 1, Buffer, out _);
        FormatterBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", EmptyTags), 1, Buffer, out _);
        FormatterBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", EmptyTags), 1, Buffer, out _);
    }

    [Benchmark]
    public void BufferBasedWithTagsFormatterAndNullTags()
    {
        FormatterWithTagsBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", null), 1, Buffer, out _);
        FormatterWithTagsBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", null), 1, Buffer, out _);
        FormatterWithTagsBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", null), 1, Buffer, out _);
    }

    [Benchmark]
    public void BufferBasedWithTagsFormatter()
    {
        FormatterWithTagsBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", AnyValidTags), 1, Buffer, out _);
        FormatterWithTagsBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", AnyValidTags), 1, Buffer, out _);
        FormatterWithTagsBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", AnyValidTags), 1, Buffer, out _);
    }

    [Benchmark]
    public void BufferBasedWithTagsFormatterAndEmptyTags()
    {
        FormatterWithTagsBuffer.TryFormat(StatsDMessage.Gauge(255, "some.neat.bucket", EmptyTags), 1, Buffer, out _);
        FormatterWithTagsBuffer.TryFormat(StatsDMessage.Timing(255, "some.neat.bucket", EmptyTags), 1, Buffer, out _);
        FormatterWithTagsBuffer.TryFormat(StatsDMessage.Counter(255, "some.neat.bucket", EmptyTags), 1, Buffer, out _);
    }
}
