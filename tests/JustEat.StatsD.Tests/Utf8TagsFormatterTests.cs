using System.Text;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.TagsFormatters;

namespace JustEat.StatsD;

public static class Utf8TagsFormatterTests
{
    private static readonly byte[] Buffer = new byte[512];
    private static readonly Dictionary<string, string?> AnyValidTags = new Dictionary<string, string?>
    {
        ["foo"] = "bar",
        ["empty"] = null,
        ["lorem"] = "ipsum",
    };

    [Theory]
    [InlineData(TagsFormatter.NoOp, "prefix.bucket:128|c|@0.5")]
    [InlineData(TagsFormatter.Trailing, "prefix.bucket:128|c|@0.5|#foo:bar,empty,lorem:ipsum")]
    [InlineData(TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|c|@0.5")]
    [InlineData(TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|c|@0.5")]
    [InlineData(TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|c|@0.5")]
    public static void CounterSampled(TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Counter(128, "bucket", AnyValidTags);
        Check(message, 0.5, tagsFormatter, expected);
    }

    [Theory]
    [InlineData(TagsFormatter.NoOp, "prefix.bucket:128|c")]
    [InlineData(TagsFormatter.Trailing, "prefix.bucket:128|c|#foo:bar,empty,lorem:ipsum")]
    [InlineData(TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|c")]
    [InlineData(TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|c")]
    [InlineData(TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|c")]
    public static void CounterRegular(TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Counter(128, "bucket", AnyValidTags);
        Check(message, tagsFormatter, expected);
    }

    [Theory]
    [InlineData(TagsFormatter.NoOp, "prefix.bucket:-128|c")]
    [InlineData(TagsFormatter.Trailing, "prefix.bucket:-128|c|#foo:bar,empty,lorem:ipsum")]
    [InlineData(TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:-128|c")]
    [InlineData(TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:-128|c")]
    [InlineData(TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:-128|c")]
    public static void CounterNegative(TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Counter(-128, "bucket", AnyValidTags);
        Check(message, tagsFormatter, expected);
    }

    [Theory]
    [InlineData(TagsFormatter.NoOp, "prefix.bucket:128|c")]
    [InlineData(TagsFormatter.Trailing, "prefix.bucket:128|c")]
    [InlineData(TagsFormatter.InfluxDb, "prefix.bucket:128|c")]
    [InlineData(TagsFormatter.Librato, "prefix.bucket:128|c")]
    [InlineData(TagsFormatter.SignalFx, "prefix.bucket:128|c")]
    public static void CounterWithoutTags(TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Counter(128, "bucket", null);
        Check(message, tagsFormatter, expected);
    }

    [Theory]
    [InlineData(TagsFormatter.NoOp, "prefix.bucket:128|ms")]
    [InlineData(TagsFormatter.Trailing, "prefix.bucket:128|ms|#foo:bar,empty,lorem:ipsum")]
    [InlineData(TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|ms")]
    [InlineData(TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|ms")]
    [InlineData(TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|ms")]
    public static void Timing(TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Timing(128, "bucket", AnyValidTags);
        Check(message, tagsFormatter, expected);
    }

    [Theory]
    [InlineData(TagsFormatter.NoOp, "prefix.bucket:128|ms|@0.5")]
    [InlineData(TagsFormatter.Trailing, "prefix.bucket:128|ms|@0.5|#foo:bar,empty,lorem:ipsum")]
    [InlineData(TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|ms|@0.5")]
    [InlineData(TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|ms|@0.5")]
    [InlineData(TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|ms|@0.5")]
    public static void TimingSampled(TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Timing(128, "bucket", AnyValidTags);
        Check(message, 0.5, tagsFormatter, expected);
    }

    [Theory]
    [InlineData(TagsFormatter.NoOp, "prefix.bucket:128|g")]
    [InlineData(TagsFormatter.Trailing, "prefix.bucket:128|g|#foo:bar,empty,lorem:ipsum")]
    [InlineData(TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|g")]
    [InlineData(TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|g")]
    [InlineData(TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|g")]
    public static void GaugeIntegral(TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Gauge(128, "bucket", AnyValidTags);
        Check(message, tagsFormatter, expected);
    }

    [Theory]
    [InlineData(TagsFormatter.NoOp, "prefix.bucket:128.5|g")]
    [InlineData(TagsFormatter.Trailing, "prefix.bucket:128.5|g|#foo:bar,empty,lorem:ipsum")]
    [InlineData(TagsFormatter.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128.5|g")]
    [InlineData(TagsFormatter.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128.5|g")]
    [InlineData(TagsFormatter.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128.5|g")]
    public static void GaugeFloat(TagsFormatter tagsFormatter, string expected)
    {
        var message = StatsDMessage.Gauge(128.5, "bucket", AnyValidTags);
        Check(message, tagsFormatter, expected);
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

        var expected = $"prefix.{hugeBucket}:128.5|g|#{hugeTag}";

        var anyTagsFormatter = TagsFormatter.Trailing;
        var formatter = GetStatsDUtf8Formatter(anyTagsFormatter);

        // Act
        var buffer = new byte[formatter.GetMaxBufferSize(message)];

        // Assert
        formatter.TryFormat(message, 1.0, buffer, out int written).ShouldBe(true);
        var actual = Encoding.UTF8.GetString(buffer.AsSpan(0, written));
        actual.ShouldBe(expected);
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

        var expected = $"prefix.{hugeBucket}:128.5|g";

        var anyTagsFormatter = TagsFormatter.Trailing;
        var formatter = GetStatsDUtf8Formatter(anyTagsFormatter);

        // Act
        var buffer = new byte[formatter.GetMaxBufferSize(message)];

        // Assert
        formatter.TryFormat(message, 1.0, buffer, out int written).ShouldBe(true);
        var actual = Encoding.UTF8.GetString(buffer.AsSpan(0, written));
        actual.ShouldBe(expected);
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

        var expected = $"prefix.{hugeBucket}:128.5|g";

        var formatter = GetStatsDUtf8Formatter();

        // Act
        var buffer = new byte[formatter.GetMaxBufferSize(message)];

        // Assert
        formatter.TryFormat(message, 1.0, buffer, out int written).ShouldBe(true);
        var actual = Encoding.UTF8.GetString(buffer.AsSpan(0, written));
        actual.ShouldBe(expected);
    }

    [Fact]
    public static void CounterWithNullFormattedTags()
    {
        // Arrange
        var nullTagsFormatterMock = new AlwaysNullStatsDTagsFormatter();
        var formatter = GetStatsDUtf8Formatter(nullTagsFormatterMock);
        var message = StatsDMessage.Counter(128, "bucket", AnyValidTags);

        // Act and assert
        Check(message, 0.5, formatter, "prefix.bucket:128|c|@0.5");
    }

    [Fact]
    public static void TimingWithNullFormattedTags()
    {
        // Arrange
        var nullTagsFormatterMock = new AlwaysNullStatsDTagsFormatter();
        var formatter = GetStatsDUtf8Formatter(nullTagsFormatterMock);
        var message = StatsDMessage.Timing(128, "bucket", AnyValidTags);

        // Act and assert
        Check(message, 0.5, formatter, "prefix.bucket:128|ms|@0.5");
    }

    [Fact]
    public static void GaugeWithNullFormattedTags()
    {
        // Arrange
        var nullTagsFormatterMock = new AlwaysNullStatsDTagsFormatter();
        var formatter = GetStatsDUtf8Formatter(nullTagsFormatterMock);
        var message = StatsDMessage.Gauge(128, "bucket", AnyValidTags);

        // Act and assert
        Check(message, formatter, "prefix.bucket:128|g");
    }

    private static void Check(StatsDMessage message, TagsFormatter tagsFormatter, string expected)
    {
        Check(message, 1, tagsFormatter, expected);
    }

    private static void Check(StatsDMessage message, double sampleRate, TagsFormatter tagsFormatter, string expected)
    {
        var formatter = GetStatsDUtf8Formatter(tagsFormatter);
        Check(message, sampleRate, formatter, expected);
    }

    private static void Check(StatsDMessage message, StatsDUtf8Formatter formatter, string expected)
    {
        Check(message, 1, formatter, expected);
    }

    private static void Check(StatsDMessage message, double sampleRate, StatsDUtf8Formatter formatter, string expected)
    {
        formatter.TryFormat(message, sampleRate, Buffer, out int written).ShouldBe(true);
        var result = Encoding.UTF8.GetString(Buffer.AsSpan(0, written));
        result.ShouldBe(expected);
    }

    private static StatsDUtf8Formatter GetStatsDUtf8Formatter(TagsFormatter tagsFormatter = TagsFormatter.NoOp)
    {
        IStatsDTagsFormatter statsDTagsFormatter = tagsFormatter switch
        {
            TagsFormatter.NoOp => new NoOpTagsFormatter(),
            TagsFormatter.Trailing => JustEat.StatsD.TagsFormatter.CloudWatch,
            TagsFormatter.InfluxDb => JustEat.StatsD.TagsFormatter.InfluxDb,
            TagsFormatter.Librato => JustEat.StatsD.TagsFormatter.Librato,
            TagsFormatter.SignalFx => JustEat.StatsD.TagsFormatter.SignalFx,
            _ => throw new ArgumentOutOfRangeException(nameof(tagsFormatter))
        };

        return GetStatsDUtf8Formatter(statsDTagsFormatter);
    }

    private static StatsDUtf8Formatter GetStatsDUtf8Formatter(IStatsDTagsFormatter tagsFormatter)
    {
        return new StatsDUtf8Formatter("prefix", tagsFormatter);
    }

    public enum TagsFormatter
    {
        NoOp,
        Trailing,
        InfluxDb,
        Librato,
        SignalFx,
    }

    private class AlwaysNullStatsDTagsFormatter : IStatsDTagsFormatter
    {
        public bool AreTrailing { get; }

        public int GetTagsBufferSize(in Dictionary<string, string?> tags) => 0;

        public ReadOnlySpan<char> FormatTags(in Dictionary<string, string?> tags) => null;
    }
}
