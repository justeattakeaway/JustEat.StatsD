using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class StatSendingBenchmark
    {
        private BufferBasedStatsDPublisher _pooledUdpSender;
        private StringBasedStatsDPublisher _ipSender;

        private static readonly TimeSpan Timed = TimeSpan.FromMinutes(1);

        [GlobalSetup]
        public void Setup()
        {
            var config = new StatsDConfiguration
            {
                // if you want verify that stats are received,
                // you will need the IP of suitable local test stats server
                Host = "127.0.0.1",
                Prefix = "testmetric"
            };

            var endpointSource = EndpointParser.MakeEndPointSource(
                config.Host, config.Port, config.DnsLookupInterval);

            var ipTransport = new IpTransport(endpointSource);
            _ipSender = new StringBasedStatsDPublisher(config, ipTransport);
            _ipSender.Increment("startup.ip");

            var pooledUdpTransport = new PooledUdpTransport(endpointSource);
            _pooledUdpSender = new BufferBasedStatsDPublisher(config, pooledUdpTransport);
            _pooledUdpSender.Increment("startup.v2");
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
        public void RunBuffered()
        {
            _pooledUdpSender.MarkEvent("hello.v2");
            _pooledUdpSender.Increment(20, "increment.v2");
            _pooledUdpSender.Timing(Timed, "timer.v2");
            _pooledUdpSender.Gauge(354654, "gauge.v2");
            _pooledUdpSender.Gauge(25.1, "free-space.v2");
        }

        [Benchmark]
        public void RunBufferedWithSampling()
        {
            _pooledUdpSender.Increment(2, 0.2, "increment.v2");
            _pooledUdpSender.Timing(2, 0.2, "increment.v2");
        }
    }
}
