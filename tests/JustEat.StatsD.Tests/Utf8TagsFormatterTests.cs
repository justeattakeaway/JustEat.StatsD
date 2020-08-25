using System;
using System.Collections.Generic;
using System.Text;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.Buffered.Tags;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class Utf8TagsFormatterTests
    {
        private static readonly byte[] Buffer = new byte[512];
        private static readonly IDictionary<string, string?> AnyValidTags = new Dictionary<string, string?>
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
        [InlineData(16, 'z')]
        [InlineData(256, 'z')]
        public static void GetMaxBufferSizeCalculatesValidBufferSizesWithTags(int bucketSize, char ch)
        {
            var hugeBucket = new string(ch, bucketSize);
            var message = StatsDMessage.Gauge(128.5, hugeBucket, AnyValidTags);
            var expected = $"prefix.{hugeBucket}:128.5|g|#foo:bar,empty,lorem:ipsum";
            var anyTagsFormatter = TagsFormatter.Trailing;
            var formatter = GetStatsDUtf8Formatter(anyTagsFormatter);

            var buffer = new byte[formatter.GetMaxBufferSize(message)];

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
            var hugeBucket = new string(ch, bucketSize);
            var message = StatsDMessage.Gauge(128.5, hugeBucket, null);
            var expected = $"prefix.{hugeBucket}:128.5|g";
            var anyTagsFormatter = TagsFormatter.Trailing;
            var formatter = GetStatsDUtf8Formatter(anyTagsFormatter);

            var buffer = new byte[formatter.GetMaxBufferSize(message)];

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
            var hugeBucket = new string(ch, bucketSize);
            var message = StatsDMessage.Gauge(128.5, hugeBucket, AnyValidTags);
            var expected = $"prefix.{hugeBucket}:128.5|g";
            var formatter = GetStatsDUtf8Formatter();

            var buffer = new byte[formatter.GetMaxBufferSize(message)];

            formatter.TryFormat(message, 1.0, buffer, out int written).ShouldBe(true);
            var actual = Encoding.UTF8.GetString(buffer.AsSpan(0, written));
            actual.ShouldBe(expected);
        }

        private static void Check(StatsDMessage message, TagsFormatter tagsFormatter, string expected)
        {
            Check(message, 1, tagsFormatter, expected);
        }

        private static void Check(StatsDMessage message, double sampleRate, TagsFormatter tagsFormatter, string expected)
        {
            var formatter = GetStatsDUtf8Formatter(tagsFormatter);
            formatter.TryFormat(message, sampleRate, Buffer, out int written).ShouldBe(true);
            var result = Encoding.UTF8.GetString(Buffer.AsSpan(0, written));
            result.ShouldBe(expected);
        }

        private static StatsDUtf8Formatter GetStatsDUtf8Formatter(TagsFormatter tagsFormatter = TagsFormatter.NoOp)
        {
            IStatsDTagsFormatter statsDTagsFormatter = tagsFormatter switch
            {
                TagsFormatter.NoOp => new NoOpTagsFormatter(),
                TagsFormatter.Trailing => new TrailingTagsFormatter(),
                TagsFormatter.InfluxDb => new InfluxDbTagsFormatter(),
                TagsFormatter.Librato => new LibratoTagsFormatter(),
                TagsFormatter.SignalFx => new SignalFxTagsFormatter(),
                _ => throw new ArgumentOutOfRangeException(nameof(tagsFormatter))
            };

            return new StatsDUtf8Formatter("prefix", statsDTagsFormatter);
        }

        public enum TagsFormatter
        {
            NoOp,
            Trailing,
            InfluxDb,
            Librato,
            SignalFx,
        }
    }
}
