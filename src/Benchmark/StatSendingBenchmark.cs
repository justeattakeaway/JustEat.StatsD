using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Configs;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class StatSendingBenchmark
    {
        private IStatsDPublisher _udpSender;
        private IStatsDPublisher _ipSender;

        private IStatsDPublisher _udpSenderSpan;
        private IStatsDPublisher _ipSenderSpan;

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
            var udpTransportPooled = new UdpTransportPooled(endpointSource);
            var ipTransport = new IpTransport(endpointSource);

            _udpSender = new StatsDPublisher(config, udpTransport);
            _udpSender.Increment("startup.ud");

            _ipSender = new StatsDPublisher(config, ipTransport);
            _ipSender.Increment("startup.ip");

            _udpSenderSpan = new SpanStatsDPublisher(config, udpTransportPooled);
            _udpSenderSpan.Increment("startup.ud");

            _ipSenderSpan = new SpanStatsDPublisher(config, ipTransport);
            _ipSenderSpan.Increment("startup.ip");
        }

        private static void Run(IStatsDPublisher sender)
        {
            sender.Increment(1, 0.5, "increment.stat");
            sender.Decrement(1, 0.5, "increment.stat");
            sender.Increment(1, "increment.stat");
            sender.Decrement(1, "increment.stat");
            sender.Timing(Timed, "timer.stat");
            sender.Timing(Timed, 0.5, "timer.stat");
            sender.Gauge(long.MinValue, "gauge.stat", DateTime.Now);
        }

        [Benchmark(Baseline = true),BenchmarkCategory("UDP")]
        public void RunUdp()
        {
            Run(_udpSender);
        }

        [Benchmark, BenchmarkCategory("UDP")]
        public void RunUdpSpanPooled()
        {
            Run(_udpSenderSpan);
        }

        [Benchmark(Baseline = true), BenchmarkCategory("TCP")]
        public void RunIp()
        {
            Run(_ipSender);
        }

        [Benchmark, BenchmarkCategory("TCP")]
        public void RunIpSpan()
        {
            Run(_ipSenderSpan);
        }
    }
}
