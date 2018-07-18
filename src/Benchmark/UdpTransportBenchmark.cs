using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class UdpTransportBenchmark
    {
        private const string MetricName = "this.is.a.metric";

        private PooledUdpTransport _pooledTransport;
        private UdpTransport _unpooledTransport;

        [GlobalSetup]
        public void Setup()
        {
            var config = new StatsDConfiguration
            {
                Host = "127.0.0.1",
            };

            var endpointSource = EndpointParser.MakeEndPointSource(
                config.Host,
                config.Port,
                config.DnsLookupInterval);

            _pooledTransport = new PooledUdpTransport(endpointSource);
            _unpooledTransport = new UdpTransport(endpointSource);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pooledTransport.Dispose();
        }

        [Benchmark(Baseline = true)]
        public void SendWithoutPool()
        {
            _unpooledTransport.Send(MetricName);
        }

        [Benchmark]
        public void SendWithPool()
        {
            _pooledTransport.Send(MetricName);
        }
    }
}
