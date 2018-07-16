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
        public string IncrementBy2WithSampling()
        {
            return _formatter.Increment(2, 0.5, "some.stat");
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
