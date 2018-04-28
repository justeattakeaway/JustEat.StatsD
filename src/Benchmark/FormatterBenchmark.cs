using BenchmarkDotNet.Attributes;
using JustEat.StatsD;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class FormatterBenchmark
    {
        private StatsDMessageFormatter _formatter;

        [GlobalSetup]
        public void Setup()
        {
            _formatter = new StatsDMessageFormatter("test.prefix");
        }

        [Benchmark]
        public string Increment()
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

}
