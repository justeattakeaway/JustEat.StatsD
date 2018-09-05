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
        private StatsDPublisherV2 _v2Publisher;
        private StatsDPublisher _ipSender;

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

            var udpTransport = new UdpTransport(endpointSource);
            _udpSender = new StatsDPublisher(config, udpTransport);
            _udpSender.Increment("startup.ud");

            var ipTransport = new IpTransport(endpointSource);
            _ipSender = new StatsDPublisher(config, ipTransport);
            _ipSender.Increment("startup.ip");

            var pooledUdpV2 = new PooledUdpTransportV2(endpointSource);
            _v2Publisher = new StatsDPublisherV2(config, pooledUdpV2);
            _v2Publisher.Increment("startup.v2");
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
        public void RunIp()
        {
            _ipSender.MarkEvent("hello.ip");
            _ipSender.Increment(20, "increment.ip");
            _ipSender.Timing(Timed, "timer.ip");
            _ipSender.Gauge(354654, "gauge.ip");
            _ipSender.Gauge(25.1, "free-space.ip");
        }

        [Benchmark]
        public void RunIpWithSampling()
        {
            _udpSender.Increment(2, 0.5, "increment.ip");
        }

        [Benchmark]
        public void RunV2()
        {
            _v2Publisher.MarkEvent("hello.v2");
            _v2Publisher.Increment(20, "increment.v2");
            _v2Publisher.Timing(Timed, "timer.v2");
            _v2Publisher.Gauge(354654, "gauge.v2");
            _v2Publisher.Gauge(25.1, "free-space.v2");
        }

        [Benchmark]
        public void RunV2WithSampling()
        {
            _v2Publisher.Increment(2, 0.5, "increment.v2");
        }
    }
}
