using System.Text;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.TagsFormatters;

namespace JustEat.StatsD;

public static class Utf8TagsFormatterTests
{
    private static readonly byte[] Buffer = new byte[512];
    private static readonly Dictionary<string, string?> AnyValidTags = new()
    {
        ["foo"] = "bar",
        ["empty"] = null,
        ["lorem"] = "ipsum",
    };

    [Theory]
    [InlineData(false, TagsFormatter.NoOp, "prefix.bucket:128|c|@0.5")]
    [InlineData(false, TagsFormatter.Trailing, "prefix.bucket:128|c|@0.5|#foo:bar,empty,lorem:ipsum")]
    [InlineData(false, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|c|@0.5")]
    [InlineData(false, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|c|@0.5")]
    [InlineData(false, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|c|@0.5")]
    [InlineData(false, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|c|@0.5")]
    [InlineData(true, TagsFormatter.NoOp, "prefix.bucket:128|c|@0.5\n")]
    [InlineData(true, TagsFormatter.Trailing, "prefix.bucket:128|c|@0.5|#foo:bar,empty,lorem:ipsum\n")]
    [InlineData(true, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|c|@0.5\n")]
    [InlineData(true, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|c|@0.5\n")]
    [InlineData(true, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|c|@0.5\n")]
    [InlineData(true, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|c|@0.5\n")]
    public static void CounterSampled(bool formatterEndWithLineFeedSymbol, TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Counter(128, "bucket", AnyValidTags);
        Check(message, 0.5, tagsFormatter, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, TagsFormatter.NoOp, "prefix.bucket:128|c")]
    [InlineData(false, TagsFormatter.Trailing, "prefix.bucket:128|c|#foo:bar,empty,lorem:ipsum")]
    [InlineData(false, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|c")]
    [InlineData(false, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|c")]
    [InlineData(false, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|c")]
    [InlineData(false, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|c")]
    [InlineData(true, TagsFormatter.NoOp, "prefix.bucket:128|c\n")]
    [InlineData(true, TagsFormatter.Trailing, "prefix.bucket:128|c|#foo:bar,empty,lorem:ipsum\n")]
    [InlineData(true, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|c\n")]
    [InlineData(true, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|c\n")]
    [InlineData(true, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|c\n")]
    [InlineData(true, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|c\n")]
    public static void CounterRegular(bool formatterEndWithLineFeedSymbol, TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Counter(128, "bucket", AnyValidTags);
        Check(message, tagsFormatter, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, TagsFormatter.NoOp, "prefix.bucket:-128|c")]
    [InlineData(false, TagsFormatter.Trailing, "prefix.bucket:-128|c|#foo:bar,empty,lorem:ipsum")]
    [InlineData(false, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:-128|c")]
    [InlineData(false, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:-128|c")]
    [InlineData(false, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:-128|c")]
    [InlineData(false, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:-128|c")]
    [InlineData(true, TagsFormatter.NoOp, "prefix.bucket:-128|c\n")]
    [InlineData(true, TagsFormatter.Trailing, "prefix.bucket:-128|c|#foo:bar,empty,lorem:ipsum\n")]
    [InlineData(true, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:-128|c\n")]
    [InlineData(true, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:-128|c\n")]
    [InlineData(true, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:-128|c\n")]
    [InlineData(true, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:-128|c\n")]
    public static void CounterNegative(bool formatterEndWithLineFeedSymbol, TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Counter(-128, "bucket", AnyValidTags);
        Check(message, tagsFormatter, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, TagsFormatter.NoOp, "prefix.bucket:128|c")]
    [InlineData(false, TagsFormatter.Trailing, "prefix.bucket:128|c")]
    [InlineData(false, TagsFormatter.InfluxDb, "prefix.bucket:128|c")]
    [InlineData(false, TagsFormatter.GraphiteDb, "prefix.bucket:128|c")]
    [InlineData(false, TagsFormatter.Librato, "prefix.bucket:128|c")]
    [InlineData(false, TagsFormatter.SignalFx, "prefix.bucket:128|c")]
    [InlineData(true, TagsFormatter.NoOp, "prefix.bucket:128|c\n")]
    [InlineData(true, TagsFormatter.Trailing, "prefix.bucket:128|c\n")]
    [InlineData(true, TagsFormatter.InfluxDb, "prefix.bucket:128|c\n")]
    [InlineData(true, TagsFormatter.GraphiteDb, "prefix.bucket:128|c\n")]
    [InlineData(true, TagsFormatter.Librato, "prefix.bucket:128|c\n")]
    [InlineData(true, TagsFormatter.SignalFx, "prefix.bucket:128|c\n")]
    public static void CounterWithoutTags(bool formatterEndWithLineFeedSymbol, TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Counter(128, "bucket", null);
        Check(message, tagsFormatter, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, TagsFormatter.NoOp, "prefix.bucket:128|ms")]
    [InlineData(false, TagsFormatter.Trailing, "prefix.bucket:128|ms|#foo:bar,empty,lorem:ipsum")]
    [InlineData(false, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|ms")]
    [InlineData(false, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|ms")]
    [InlineData(false, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|ms")]
    [InlineData(false, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|ms")]
    [InlineData(true, TagsFormatter.NoOp, "prefix.bucket:128|ms\n")]
    [InlineData(true, TagsFormatter.Trailing, "prefix.bucket:128|ms|#foo:bar,empty,lorem:ipsum\n")]
    [InlineData(true, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|ms\n")]
    [InlineData(true, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|ms\n")]
    [InlineData(true, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|ms\n")]
    [InlineData(true, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|ms\n")]
    public static void Timing(bool formatterEndWithLineFeedSymbol, TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Timing(128, "bucket", AnyValidTags);
        Check(message, tagsFormatter, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, TagsFormatter.NoOp, "prefix.bucket:128|ms|@0.5")]
    [InlineData(false, TagsFormatter.Trailing, "prefix.bucket:128|ms|@0.5|#foo:bar,empty,lorem:ipsum")]
    [InlineData(false, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|ms|@0.5")]
    [InlineData(false, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|ms|@0.5")]
    [InlineData(false, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|ms|@0.5")]
    [InlineData(false, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|ms|@0.5")]
    [InlineData(true, TagsFormatter.NoOp, "prefix.bucket:128|ms|@0.5\n")]
    [InlineData(true, TagsFormatter.Trailing, "prefix.bucket:128|ms|@0.5|#foo:bar,empty,lorem:ipsum\n")]
    [InlineData(true, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|ms|@0.5\n")]
    [InlineData(true, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|ms|@0.5\n")]
    [InlineData(true, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|ms|@0.5\n")]
    [InlineData(true, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|ms|@0.5\n")]
    public static void TimingSampled(bool formatterEndWithLineFeedSymbol, TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Timing(128, "bucket", AnyValidTags);
        Check(message, 0.5, tagsFormatter, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, TagsFormatter.NoOp, "prefix.bucket:128|g")]
    [InlineData(false, TagsFormatter.Trailing, "prefix.bucket:128|g|#foo:bar,empty,lorem:ipsum")]
    [InlineData(false, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|g")]
    [InlineData(false, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|g")]
    [InlineData(false, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|g")]
    [InlineData(false, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|g")]
    [InlineData(true, TagsFormatter.NoOp, "prefix.bucket:128|g\n")]
    [InlineData(true, TagsFormatter.Trailing, "prefix.bucket:128|g|#foo:bar,empty,lorem:ipsum\n")]
    [InlineData(true, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|g\n")]
    [InlineData(true, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128|g\n")]
    [InlineData(true, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|g\n")]
    [InlineData(true, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|g\n")]
    public static void GaugeIntegral(bool formatterEndWithLineFeedSymbol, TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Gauge(128, "bucket", AnyValidTags);
        Check(message, tagsFormatter, formatterEndWithLineFeedSymbol, expected);
    }

    [Theory]
    [InlineData(false, TagsFormatter.NoOp, "prefix.bucket:128.5|g")]
    [InlineData(false, TagsFormatter.Trailing, "prefix.bucket:128.5|g|#foo:bar,empty,lorem:ipsum")]
    [InlineData(false, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128.5|g")]
    [InlineData(false, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128.5|g")]
    [InlineData(false, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128.5|g")]
    [InlineData(false, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128.5|g")]
    [InlineData(true, TagsFormatter.NoOp, "prefix.bucket:128.5|g\n")]
    [InlineData(true, TagsFormatter.Trailing, "prefix.bucket:128.5|g|#foo:bar,empty,lorem:ipsum\n")]
    [InlineData(true, TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128.5|g\n")]
    [InlineData(true, TagsFormatter.GraphiteDb, "prefix.bucket;foo=bar;empty;lorem=ipsum:128.5|g\n")]
    [InlineData(true, TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128.5|g\n")]
    [InlineData(true, TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128.5|g\n")]
    public static void GaugeFloat(bool formatterEndWithLineFeedSymbol, TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Gauge(128.5, "bucket", AnyValidTags);
        Check(message, tagsFormatter, formatterEndWithLineFeedSymbol, expected);
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
    public static void GetMaxBufferSizeCalculatesValidBufferSizesWithTags(int size, char ch)
    {
        // Arrange
        var hugeBucket = new string(ch, size);
        var hugeTag = new string(ch, size);
        var anyValidTags = new Dictionary<string, string?> { [hugeTag] = null };
        var message = StatsDMessage.Gauge(128.5, hugeBucket, anyValidTags);
        var anyTagsFormatter = TagsFormatter.Trailing;

        // Act and Assert
        CheckWithBufferCalculation(message, 1.0, GetStatsDUtf8Formatter(false, anyTagsFormatter), $"prefix.{hugeBucket}:128.5|g|#{hugeTag}");
        CheckWithBufferCalculation(message, 1.0, GetStatsDUtf8Formatter(true, anyTagsFormatter), $"prefix.{hugeBucket}:128.5|g|#{hugeTag}\n");
    }

    [Theory]
    [InlineData(1, 'z')]
    [InlineData(16, 'z')]
    [InlineData(256, 'z')]
    public static void GetMaxBufferSizeCalculatesValidBufferSizesWithoutTags(int bucketSize, char ch)
    {
        // Arrange
        var hugeBucket = new string(ch, bucketSize);
        var message = StatsDMessage.Gauge(128.5, hugeBucket, null);
        var anyTagsFormatter = TagsFormatter.Trailing;

        // Act and Assert
        CheckWithBufferCalculation(message, 1.0, GetStatsDUtf8Formatter(false, anyTagsFormatter), $"prefix.{hugeBucket}:128.5|g");
        CheckWithBufferCalculation(message, 1.0, GetStatsDUtf8Formatter(true, anyTagsFormatter), $"prefix.{hugeBucket}:128.5|g\n");
    }

    [Theory]
    [InlineData(1, 'z')]
    [InlineData(16, 'z')]
    [InlineData(256, 'z')]
    public static void GetMaxBufferSizeCalculatesValidBufferSizesIgnoringTags(int bucketSize, char ch)
    {
        // Arrange
        var hugeBucket = new string(ch, bucketSize);
        var message = StatsDMessage.Gauge(128.5, hugeBucket, AnyValidTags);

        // Act and Assert
        CheckWithBufferCalculation(message, 1.0, GetStatsDUtf8Formatter(false), $"prefix.{hugeBucket}:128.5|g");
        CheckWithBufferCalculation(message, 1.0, GetStatsDUtf8Formatter(true), $"prefix.{hugeBucket}:128.5|g\n");
    }

    [Theory]
    [InlineData(false, "prefix.bucket:128|c|@0.5")]
    [InlineData(true, "prefix.bucket:128|c|@0.5\n")]
    public static void CounterWithNullFormattedTags(bool formatterEndWithLineFeedSymbol, string expected)
    {
        // Arrange
        var nullTagsFormatterMock = new AlwaysNullStatsDTagsFormatter();
        var formatter = GetStatsDUtf8Formatter(nullTagsFormatterMock, formatterEndWithLineFeedSymbol);
        var message = StatsDMessage.Counter(128, "bucket", AnyValidTags);

        // Act and assert
        Check(message, 0.5, formatter, expected);
    }

    [Theory]
    [InlineData(false, "prefix.bucket:128|ms|@0.5")]
    [InlineData(true, "prefix.bucket:128|ms|@0.5\n")]
    public static void TimingWithNullFormattedTags(bool formatterEndWithLineFeedSymbol, string expected)
    {
        // Arrange
        var nullTagsFormatterMock = new AlwaysNullStatsDTagsFormatter();
        var formatter = GetStatsDUtf8Formatter(nullTagsFormatterMock, formatterEndWithLineFeedSymbol);
        var message = StatsDMessage.Timing(128, "bucket", AnyValidTags);

        // Act and assert
        Check(message, 0.5, formatter, expected);
    }

    [Theory]
    [InlineData(false, "prefix.bucket:128|g")]
    [InlineData(true, "prefix.bucket:128|g\n")]
    public static void GaugeWithNullFormattedTags(bool formatterEndWithLineFeedSymbol, string expected)
    {
        // Arrange
        var nullTagsFormatterMock = new AlwaysNullStatsDTagsFormatter();
        var formatter = GetStatsDUtf8Formatter(nullTagsFormatterMock, formatterEndWithLineFeedSymbol);
        var message = StatsDMessage.Gauge(128, "bucket", AnyValidTags);

        // Act and assert
        Check(message, formatter, expected);
    }

    private static void CheckWithBufferCalculation(StatsDMessage message, double sampleRate, StatsDUtf8Formatter formatter, string expected)
    {
        Check(message, sampleRate, formatter, null, expected);
    }

    private static void Check(StatsDMessage message, TagsFormatter tagsFormatter, bool formatterEndWithLineFeedSymbol, string expected)
    {
        Check(message, 1, tagsFormatter, formatterEndWithLineFeedSymbol, expected);
    }

    private static void Check(StatsDMessage message, double sampleRate, TagsFormatter tagsFormatter, bool formatterEndWithLineFeedSymbol, string expected)
    {
        var formatter = GetStatsDUtf8Formatter(formatterEndWithLineFeedSymbol, tagsFormatter);
        Check(message, sampleRate, formatter, expected);
    }

    private static void Check(StatsDMessage message, StatsDUtf8Formatter formatter, string expected)
    {
        Check(message, 1, formatter, expected);
    }

    private static void Check(StatsDMessage message, double sampleRate, StatsDUtf8Formatter formatter, string expected)
    {
        Check(message, sampleRate, formatter, Buffer, expected);
    }

    private static void Check(StatsDMessage message, double sampleRate, StatsDUtf8Formatter formatter, byte[]? buffer, string expected)
    {
        buffer ??= new byte[formatter.GetMaxBufferSize(message)];
        formatter.TryFormat(message, sampleRate, buffer, out int written).ShouldBe(true);
        var result = Encoding.UTF8.GetString(buffer.AsSpan(0, written));
        result.ShouldBe(expected);
    }

    private static StatsDUtf8Formatter GetStatsDUtf8Formatter(bool formatterEndWithLineFeedSymbol, TagsFormatter tagsFormatter = TagsFormatter.NoOp)
    {
        IStatsDTagsFormatter statsDTagsFormatter = tagsFormatter switch
        {
            TagsFormatter.NoOp => new NoOpTagsFormatter(),
            TagsFormatter.Trailing => JustEat.StatsD.TagsFormatter.CloudWatch,
            TagsFormatter.InfluxDb => JustEat.StatsD.TagsFormatter.InfluxDb,
            TagsFormatter.GraphiteDb => JustEat.StatsD.TagsFormatter.GraphiteDb,
            TagsFormatter.Librato => JustEat.StatsD.TagsFormatter.Librato,
            TagsFormatter.SignalFx => JustEat.StatsD.TagsFormatter.SignalFx,
            _ => throw new ArgumentOutOfRangeException(nameof(tagsFormatter))
        };

        return GetStatsDUtf8Formatter(statsDTagsFormatter, formatterEndWithLineFeedSymbol);
    }

    private static StatsDUtf8Formatter GetStatsDUtf8Formatter(IStatsDTagsFormatter tagsFormatter, bool formatterEndWithLineFeedSymbol)
    {
        return new StatsDUtf8Formatter("prefix", tagsFormatter, formatterEndWithLineFeedSymbol);
    }

    public enum TagsFormatter
    {
        NoOp,
        Trailing,
        InfluxDb,
        GraphiteDb,
        Librato,
        SignalFx,
    }

    private sealed class AlwaysNullStatsDTagsFormatter : IStatsDTagsFormatter
    {
        public bool AreTrailing { get; }

        public int GetTagsBufferSize(in Dictionary<string, string?> tags) => 0;

        public ReadOnlySpan<char> FormatTags(in Dictionary<string, string?> tags) => null;
    }
}
