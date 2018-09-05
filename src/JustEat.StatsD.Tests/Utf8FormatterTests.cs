using System;
using System.Text;
using JustEat.StatsD.V2;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class Utf8FormatterTests
    {
        private static readonly byte[] Buffer = new byte[512];
        private static readonly StatsDUtf8Formatter Formatter = new StatsDUtf8Formatter("prefix");

        [Fact]
        public static void CounterSampled()
        {
            var message = StatsDMessage.Counter(128, "bucket");
            Check(message, 0.5, "prefix.bucket:128|c|@0.5");
        }

        [Fact]
        public static void CounterRegular()
        {
            var message = StatsDMessage.Counter(128, "bucket");
            Check(message, "prefix.bucket:128|c");
        }

        [Fact]
        public static void CounterNegative()
        {
            var message = StatsDMessage.Counter(-128, "bucket");
            Check(message, "prefix.bucket:-128|c");
        }
        
        [Fact]
        public static void Timing()
        {
            var message = StatsDMessage.Timing(128, "bucket");
            Check(message, "prefix.bucket:128|ms");
        }

        [Fact]
        public static void TimingSampled()
        {
            var message = StatsDMessage.Timing(128, "bucket");
            Check(message, 0.5, "prefix.bucket:128|ms|@0.5");
        }


        [Fact]
        public static void GaugeIntegral()
        {
            var message = StatsDMessage.Gauge(128, "bucket");
            Check(message, "prefix.bucket:128|g");
        }

        [Fact]
        public static void GaugeFloat()
        {
            var message = StatsDMessage.Gauge(128.5, "bucket");
            Check(message, "prefix.bucket:128.5|g");
        }

        private static void Check(StatsDMessage message,  string expected)
        {
            Check(message, 1, expected);
        }

        private static void Check(StatsDMessage message, double sampleRate, string expected)
        {
            Formatter.TryFormat(message, sampleRate, Buffer, out int written).ShouldBe(true);
            var result = Encoding.UTF8.GetString(Buffer.AsSpan().Slice(0, written));
            result.ShouldBe(expected);
        }
    }
}
