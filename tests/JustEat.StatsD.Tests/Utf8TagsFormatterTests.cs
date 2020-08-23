using System;
using System.Collections.Generic;
using System.Text;
using JustEat.StatsD.Buffered;
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
        [InlineData(TagsStyle.Disabled, "prefix.bucket:128|c|@0.5")]
        [InlineData(TagsStyle.DataDog, "prefix.bucket:128|c|@0.5|#foo:bar,empty,lorem:ipsum")]
        [InlineData(TagsStyle.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|c|@0.5")]
        [InlineData(TagsStyle.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|c|@0.5")]
        [InlineData(TagsStyle.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|c|@0.5")]
        public static void CounterSampled(TagsStyle tagsStyle, string expected)
        {
            var message = StatsDMessage.Counter(128, "bucket", AnyValidTags);
            Check(message, 0.5, tagsStyle, expected);
        }
        
        [Theory]
        [InlineData(TagsStyle.Disabled, "prefix.bucket:128|c")]
        [InlineData(TagsStyle.DataDog, "prefix.bucket:128|c|#foo:bar,empty,lorem:ipsum")]
        [InlineData(TagsStyle.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|c")]
        [InlineData(TagsStyle.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|c")]
        [InlineData(TagsStyle.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|c")]
        public static void CounterRegular(TagsStyle tagsStyle, string expected)
        {
            var message = StatsDMessage.Counter(128, "bucket", AnyValidTags);
            Check(message, tagsStyle, expected);
        }
        
        [Theory]
        [InlineData(TagsStyle.Disabled, "prefix.bucket:-128|c")]
        [InlineData(TagsStyle.DataDog, "prefix.bucket:-128|c|#foo:bar,empty,lorem:ipsum")]
        [InlineData(TagsStyle.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:-128|c")]
        [InlineData(TagsStyle.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:-128|c")]
        [InlineData(TagsStyle.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:-128|c")]
        public static void CounterNegative(TagsStyle tagsStyle, string expected)
        {
            var message = StatsDMessage.Counter(-128, "bucket", AnyValidTags);
            Check(message, tagsStyle, expected);
        }
        
        [Theory]
        [InlineData(TagsStyle.Disabled, "prefix.bucket:128|c")]
        [InlineData(TagsStyle.DataDog, "prefix.bucket:128|c")]
        [InlineData(TagsStyle.InfluxDb, "prefix.bucket:128|c")]
        [InlineData(TagsStyle.Librato, "prefix.bucket:128|c")]
        [InlineData(TagsStyle.SignalFx, "prefix.bucket:128|c")]
        public static void CounterWithoutTags(TagsStyle tagsStyle, string expected)
        {
            var message = StatsDMessage.Counter(128, "bucket", null);
            Check(message, tagsStyle, expected);
        }
        
        [Theory]
        [InlineData(TagsStyle.Disabled, "prefix.bucket:128|ms")]
        [InlineData(TagsStyle.DataDog, "prefix.bucket:128|ms|#foo:bar,empty,lorem:ipsum")]
        [InlineData(TagsStyle.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|ms")]
        [InlineData(TagsStyle.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|ms")]
        [InlineData(TagsStyle.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|ms")]
        public static void Timing(TagsStyle tagsStyle, string expected)
        {
            var message = StatsDMessage.Timing(128, "bucket", AnyValidTags);
            Check(message, tagsStyle, expected);
        }
        
        [Theory]
        [InlineData(TagsStyle.Disabled, "prefix.bucket:128|ms|@0.5")]
        [InlineData(TagsStyle.DataDog, "prefix.bucket:128|ms|@0.5|#foo:bar,empty,lorem:ipsum")]
        [InlineData(TagsStyle.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|ms|@0.5")]
        [InlineData(TagsStyle.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|ms|@0.5")]
        [InlineData(TagsStyle.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|ms|@0.5")]
        public static void TimingSampled(TagsStyle tagsStyle, string expected)
        {
            var message = StatsDMessage.Timing(128, "bucket", AnyValidTags);
            Check(message, 0.5, tagsStyle, expected);
        }
        
        [Theory]
        [InlineData(TagsStyle.Disabled, "prefix.bucket:128|g")]
        [InlineData(TagsStyle.DataDog, "prefix.bucket:128|g|#foo:bar,empty,lorem:ipsum")]
        [InlineData(TagsStyle.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128|g")]
        [InlineData(TagsStyle.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128|g")]
        [InlineData(TagsStyle.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128|g")]
        public static void GaugeIntegral(TagsStyle tagsStyle, string expected)
        {
            var message = StatsDMessage.Gauge(128, "bucket", AnyValidTags);
            Check(message, tagsStyle, expected);
        }
        
        [Theory]
        [InlineData(TagsStyle.Disabled, "prefix.bucket:128.5|g")]
        [InlineData(TagsStyle.DataDog, "prefix.bucket:128.5|g|#foo:bar,empty,lorem:ipsum")]
        [InlineData(TagsStyle.InfluxDb, "prefix.bucket,foo=bar,empty,lorem=ipsum:128.5|g")]
        [InlineData(TagsStyle.Librato, "prefix.bucket#foo=bar,empty,lorem=ipsum:128.5|g")]
        [InlineData(TagsStyle.SignalFx, "prefix.bucket[foo=bar,empty,lorem=ipsum]:128.5|g")]
        public static void GaugeFloat(TagsStyle tagsStyle, string expected)
        {
            var message = StatsDMessage.Gauge(128.5, "bucket", AnyValidTags);
            Check(message, tagsStyle, expected);
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
            var anyTagsStyle = TagsStyle.DataDog;
            var formatter = new StatsDUtf8Formatter("prefix", anyTagsStyle);

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
            var anyTagsStyle = TagsStyle.DataDog;
            var formatter = new StatsDUtf8Formatter("prefix", anyTagsStyle);

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
            var formatter = new StatsDUtf8Formatter("prefix", TagsStyle.Disabled);

            var buffer = new byte[formatter.GetMaxBufferSize(message)];

            formatter.TryFormat(message, 1.0, buffer, out int written).ShouldBe(true);
            var actual = Encoding.UTF8.GetString(buffer.AsSpan(0, written));
            actual.ShouldBe(expected);
        }

        private static void Check(StatsDMessage message, TagsStyle tagsStyle, string expected)
        {
            Check(message, 1, tagsStyle, expected);
        }

        private static void Check(StatsDMessage message, double sampleRate, TagsStyle tagsStyle, string expected)
        {
            var formatter = new StatsDUtf8Formatter("prefix", tagsStyle);
            formatter.TryFormat(message, sampleRate, Buffer, out int written).ShouldBe(true);
            var result = Encoding.UTF8.GetString(Buffer.AsSpan(0, written));
            result.ShouldBe(expected);
        }
    }
}
