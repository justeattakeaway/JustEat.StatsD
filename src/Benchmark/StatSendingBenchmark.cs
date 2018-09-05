using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;
using JustEat.StatsD.V2;

namespace Benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class StatSendingBenchmark
    {
        private StatsDPublisher _udpSender;
        private StatsDPublisherV2 _newStatsDPublisher;

        private static readonly TimeSpan Timed = TimeSpan.FromMinutes(1);

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
                config.Host, config.Port, config.DnsLookupInterval);

            var udpTransport = new PooledUdpTransport(endpointSource);

            _udpSender = new StatsDPublisher(config, udpTransport);
            _udpSender.Increment("startup.ud");
            
            _newStatsDPublisher = new StatsDPublisherV2(config);
            _newStatsDPublisher.Increment("startup.ip");
        }

        [Benchmark]
        public void RunUdp()
        {
            _udpSender.MarkEvent("hello.ud");
            _udpSender.Increment(20, "increment.ud");
            _udpSender.Timing(Timed, "timer.ud");
            _udpSender.Gauge(354654, "gauge.ud");
            _udpSender.Gauge(25.1, "free-space.ud");
        }

        [Benchmark]
        public void RunUdpWithSampling()
        {
            _udpSender.Increment(2, 0.5, "increment.ud");
        }

        [Benchmark]
        public void RunV2()
        {
            _newStatsDPublisher.MarkEvent("hello.ip");
            _newStatsDPublisher.Increment(20, "increment.ip");
            _newStatsDPublisher.Timing(Timed, "timer.ip");
            _newStatsDPublisher.Gauge(354654, "gauge.ip");
            _newStatsDPublisher.Gauge(25.1, "free-space.ip");
        }

        [Benchmark]
        public void RunV2WithSampling()
        {
            _newStatsDPublisher.Increment(2, 0.5, "increment.ip");
        }
    }
}
