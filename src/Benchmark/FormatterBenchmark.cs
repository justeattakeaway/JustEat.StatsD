using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class FormatterBenchmark
    {
        private readonly StatsDMessageFormatter _formatter = new StatsDMessageFormatter("test.prefix");

        [Benchmark]
        public string IncrementBy1()
        {
            return _formatter.Increment(1, "some.stat");
        }

        [Benchmark]
        public string IncrementBy12()
        {
            return _formatter.Increment(12, "some.stat");
        }

        [Benchmark]
        public string Decrement()
        {
            return _formatter.Decrement(12, "some.stat");
        }

        [Benchmark]
        public string Event()
        {
            return _formatter.Event("some.stat");
        }

        [Benchmark]
        public string Gauge()
        {
            return _formatter.Gauge(12, "some.stat");
        }

        [Benchmark]
        public string Timing()
        {
            return _formatter.Timing(1000, "some.stat");
        }
    }

    [MemoryDiagnoser]
    public class FormatterBenchmarkSpan
    {
        private readonly SpanStatsDMessageFormatter _formatter = new SpanStatsDMessageFormatter("test.prefix");

        [Benchmark]
        public void IncrementBy1()
        {
            Span<byte> buff = stackalloc byte[64];
            var writer = new FixedBuffer(buff);
            _formatter.Increment(1, "some.stat", ref writer);
        }

        [Benchmark]
        public void IncrementBy12()
        {
            Span<byte> buff = stackalloc byte[64];
            var writer = new FixedBuffer(buff);
            _formatter.Increment(12, "some.stat", ref writer);
        }

        [Benchmark]
        public void Decrement()
        {
            Span<byte> buff = stackalloc byte[64];
            var writer = new FixedBuffer(buff);
            _formatter.Decrement(12, "some.stat", ref writer);
        }

        [Benchmark]
        public void Event()
        {
            Span<byte> buff = stackalloc byte[64];
            var writer = new FixedBuffer(buff);
            _formatter.Event("some.stat", ref writer);
        }

        [Benchmark]
        public void Gauge()
        {
            Span<byte> buff = stackalloc byte[64];
            var writer = new FixedBuffer(buff);
            _formatter.Gauge(12, "some.stat", ref writer);
        }

        [Benchmark]
        public void Timing()
        {
            Span<byte> buff = stackalloc byte[64];
            var writer = new FixedBuffer(buff);
            _formatter.Timing(1000, "some.stat", ref writer);
        }
    }

}
