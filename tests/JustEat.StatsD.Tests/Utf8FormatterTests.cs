using System.Text;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.TagsFormatters;

namespace JustEat.StatsD;

public static class Utf8FormatterTests
{
    private static readonly byte[] Buffer = new byte[512];
    private static readonly StatsDUtf8Formatter Formatter = new StatsDUtf8Formatter("prefix", new NoOpTagsFormatter());
    private static readonly StatsDUtf8Formatter LineFeedEndFormatter = new StatsDUtf8Formatter("prefix", new NoOpTagsFormatter(), true);

    [Theory]
    [InlineData(false, "prefix.bucket:128|c|@0.5")]
    [InlineData(true, "prefix.bucket:128|c|@0.5\n")]
    public static void CounterSampled(bool formatterEndWithLineFeedSymbol, string expected)
    {
        var message = StatsDMessage.Counter(128, "bucket", null);
        Check(message, 0.5, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, "prefix.bucket:128|c")]
    [InlineData(true, "prefix.bucket:128|c\n")]
    public static void CounterRegular(bool formatterEndWithLineFeedSymbol, string expected)
    {
        var message = StatsDMessage.Counter(128, "bucket", null);
        Check(message, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, "prefix.bucket:-128|c")]
    [InlineData(true, "prefix.bucket:-128|c\n")]
    public static void CounterNegative(bool formatterEndWithLineFeedSymbol, string expected)
    {
        var message = StatsDMessage.Counter(-128, "bucket", null);
        Check(message, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, "prefix.bucket:128|ms")]
    [InlineData(true, "prefix.bucket:128|ms\n")]
    public static void Timing(bool formatterEndWithLineFeedSymbol, string expected)
    {
        var message = StatsDMessage.Timing(128, "bucket", null);
        Check(message, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, "prefix.bucket:128|ms|@0.5")]
    [InlineData(true, "prefix.bucket:128|ms|@0.5\n")]
    public static void TimingSampled(bool formatterEndWithLineFeedSymbol, string expected)
    {
        var message = StatsDMessage.Timing(128, "bucket", null);
        Check(message, 0.5, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, "prefix.bucket:128|g")]
    [InlineData(true, "prefix.bucket:128|g\n")]
    public static void GaugeIntegral(bool formatterEndWithLineFeedSymbol, string expected)
    {
        var message = StatsDMessage.Gauge(128, "bucket", null);
        Check(message, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, "prefix.bucket:128.5|g")]
    [InlineData(true, "prefix.bucket:128.5|g\n")]
    public static void GaugeFloat(bool formatterEndWithLineFeedSymbol, string expected)
    {
        var message = StatsDMessage.Gauge(128.5, "bucket", null);
        Check(message, formatterEndWithLineFeedSymbol, expected);
    }

    [Fact]
    public static void MessagesLargerThenAvailableBufferShouldNotBeFormatted()
    {
        var buffer = new byte[128];
        var hugeBucket = new string('x', 256);
        var message = StatsDMessage.Gauge(128.5, hugeBucket, null);
        Formatter.TryFormat(message, 1.0, buffer, out int written).ShouldBe(false);
        written.ShouldBe(0);
    }

    [Theory]
    [InlineData(1, 'z')]
    [InlineData(2, 'z')]
    [InlineData(4, 'z')]
    [InlineData(8, 'z')]
    [InlineData(16, 'z')]
    [InlineData(32, 'z')]
    [InlineData(64, 'z')]
    [InlineData(128, 'z')]
    [InlineData(256, 'z')]
    [InlineData(512, 'z')]
    [InlineData(1024, 'z')]
    [InlineData(2048, 'z')]
    [InlineData(1, 'Ж')]
    [InlineData(2, 'Ж')]
    [InlineData(4, 'Ж')]
    [InlineData(8, 'Ж')]
    [InlineData(16, 'Ж')]
    [InlineData(32, 'Ж')]
    [InlineData(64, 'Ж')]
    [InlineData(128, 'Ж')]
    [InlineData(256, 'Ж')]
    [InlineData(512, 'Ж')]
    [InlineData(1024, 'Ж')]
    [InlineData(2048, 'Ж')]
    public static void GetMaxBufferSizeCalculatesValidBufferSizes(int bucketSize, char ch)
    {
        var hugeBucket = new string(ch, bucketSize);
        var message = StatsDMessage.Gauge(128.5, hugeBucket, null);

        CheckWithBufferCalculation(message, 1.0, false, $"prefix.{hugeBucket}:128.5|g");
        CheckWithBufferCalculation(message, 1.0, true, $"prefix.{hugeBucket}:128.5|g\n");
    }

    private static void CheckWithBufferCalculation(StatsDMessage message, double sampleRate, bool formatterEndWithLineFeedSymbol, string expected)
    {
        Check(message, sampleRate, formatterEndWithLineFeedSymbol, null, expected);
    }

    private static void Check(StatsDMessage message, bool formatterEndWithLineFeedSymbol, string expected)
    {
        Check(message, 1, formatterEndWithLineFeedSymbol, expected);
    }

    private static void Check(StatsDMessage message, double sampleRate, bool formatterEndWithLineFeedSymbol, string expected)
    {
        Check(message, sampleRate, formatterEndWithLineFeedSymbol, Buffer, expected);
    }

    private static void Check(StatsDMessage message, double sampleRate, bool formatterEndWithLineFeedSymbol, byte[]? buffer, string expected)
    {
        var formatter = SelectFormatter(formatterEndWithLineFeedSymbol);
        buffer ??= new byte[formatter.GetMaxBufferSize(message)];
        formatter.TryFormat(message, sampleRate, buffer, out int written).ShouldBe(true);
        var result = Encoding.UTF8.GetString(buffer.AsSpan(0, written));
        result.ShouldBe(expected);
    }

    private static StatsDUtf8Formatter SelectFormatter(bool formatterEndWithLineFeedSymbol) =>
        formatterEndWithLineFeedSymbol ? LineFeedEndFormatter : Formatter;
}
