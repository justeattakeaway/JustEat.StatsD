using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class StatSendingBenchmark
    {
        private BufferBasedStatsDPublisher _pooledUdpSender;
        private StringBasedStatsDPublisher _udpSender;
        private StringBasedStatsDPublisher _ipSender;

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
            _udpSender = new StringBasedStatsDPublisher(config, udpTransport);
            _udpSender.Increment("startup.ud");

            var ipTransport = new IpTransport(endpointSource);
            _ipSender = new StringBasedStatsDPublisher(config, ipTransport);
            _ipSender.Increment("startup.ip");

            var pooledUdpTransport = new PooledUdpTransport(endpointSource);
            _pooledUdpSender = new BufferBasedStatsDPublisher(config, pooledUdpTransport);
            _pooledUdpSender.Increment("startup.v2");
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
            _udpSender.Increment(2, 0.2, "increment.ud");
            _udpSender.Timing(2, 0.2, "increment.ud");
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
            _udpSender.Increment(2, 0.2, "increment.ip");
            _udpSender.Timing(2, 0.2, "increment.ip");
        }

        [Benchmark]
        public void RunV2()
        {
            _pooledUdpSender.MarkEvent("hello.v2");
            _pooledUdpSender.Increment(20, "increment.v2");
            _pooledUdpSender.Timing(Timed, "timer.v2");
            _pooledUdpSender.Gauge(354654, "gauge.v2");
            _pooledUdpSender.Gauge(25.1, "free-space.v2");
        }

        [Benchmark]
        public void RunV2WithSampling()
        {
            _pooledUdpSender.Increment(2, 0.2, "increment.v2");
            _pooledUdpSender.Timing(2, 0.2, "increment.v2");
        }
    }
}
