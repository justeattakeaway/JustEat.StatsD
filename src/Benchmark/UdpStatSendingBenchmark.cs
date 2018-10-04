using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class UdpStatSendingBenchmark
    {
        private static readonly TimeSpan Timed = TimeSpan.FromMinutes(1);

        private PooledUdpTransport _pooledTransport;
        private StringBasedStatsDPublisher _pooledUdpSender;
        private BufferBasedStatsDPublisher _bufferBasedStatsDPublisher;
        private IStatsDPublisher _adaptedStatsDPublisher;

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

            _pooledUdpSender = new StringBasedStatsDPublisher(config, _pooledTransport);
            _pooledUdpSender.Increment("startup.ud");

            _adaptedStatsDPublisher = new StatsDPublisher(config);
            _adaptedStatsDPublisher.Increment("startup.ud");

            _bufferBasedStatsDPublisher = new BufferBasedStatsDPublisher(config, _pooledTransport);
            _bufferBasedStatsDPublisher.Increment("startup.ud");
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pooledTransport.Dispose();
        }

        [Benchmark]
        public void SendStatPooledUdp()
        {
            _pooledUdpSender.Increment("increment.ud");
            _pooledUdpSender.Timing(Timed, "timer.ud");
        }

        [Benchmark]
        public void SendStatPooledUdpBuffered()
        {
            _bufferBasedStatsDPublisher.Increment("increment.ud");
            _bufferBasedStatsDPublisher.Timing(Timed, "timer.ud");
        }

        [Benchmark]
        public void SendStatPooledUdpCoveredByAdapter()
        {
            _adaptedStatsDPublisher.Increment("increment.ud");
            _adaptedStatsDPublisher.Timing(Timed, "timer.ud");
        }
    }
}
