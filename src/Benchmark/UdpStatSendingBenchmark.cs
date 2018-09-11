using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class UdpStatSendingBenchmark
    {
        private static readonly TimeSpan Timed = TimeSpan.FromMinutes(1);

        private PooledUdpTransport _pooledTransport;
        private StringBasedStatsDPublisher _udpSender;
        private StringBasedStatsDPublisher _pooledUdpSender;

        [GlobalSetup]
        public void Setup()
        {
            var config = new StatsDConfiguration
            {
                // if you want verify that stats are recevied,
                // you will need the IP of suitable local test stats server
                Host = "127.0.0.1",
                Prefix = "testmetric"
            };

            var endpointSource = EndpointParser.MakeEndPointSource(
                config.Host,
                config.Port,
                config.DnsLookupInterval);

            _pooledTransport = new PooledUdpTransport(endpointSource);
            var udpTransport = new UdpTransport(endpointSource);

            _udpSender = new StringBasedStatsDPublisher(config, udpTransport);
            _udpSender.Increment("startup.ud");

            _pooledUdpSender = new StringBasedStatsDPublisher(config, _pooledTransport);
            _pooledUdpSender.Increment("startup.ud");
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pooledTransport.Dispose();
        }

        [Benchmark(Baseline = true)]
        public void SendStatUdp()
        {
            _udpSender.Increment("increment.ud");
            _udpSender.Timing(Timed, "timer.ud");
        }

        [Benchmark]
        public void SendStatPooledUdp()
        {
            _pooledUdpSender.Increment("increment.ud");
            _pooledUdpSender.Timing(Timed, "timer.ud");
        }
    }
}
