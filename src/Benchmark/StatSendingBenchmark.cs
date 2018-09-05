using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class StatSendingBenchmark
    {
        private StatsDPublisher _udpSender;
        private StatsDPublisher _ipSender;

        private static readonly TimeSpan Timed = TimeSpan.FromMinutes(1);
        private StatsDPublisher _nullSender;

        private class NullTransport : IStatsDTransport
        {
            public void Send(in Data metric)
            {

            }
        }

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
            var ipTransport = new IpTransport(endpointSource);

            _udpSender = new StatsDPublisher(config, udpTransport);
            _udpSender.Increment("startup.ud");

            _ipSender = new StatsDPublisher(config, ipTransport);
            _ipSender.Increment("startup.ip");

            _nullSender = new StatsDPublisher(config, new NullTransport());

        }

        [Benchmark]
        public void RunNull()
        {
            _nullSender.Increment("increment.null");
            _nullSender.Timing(Timed, "timer.null");
            _nullSender.Gauge(long.MaxValue / 2, "timer.null", DateTime.Now);
        }

        [Benchmark]
        public void RunUdp()
        {
            _udpSender.Increment("increment.ud");
            _udpSender.Timing(Timed, "timer.ud");
            _udpSender.Gauge(long.MaxValue / 2, "timer.ud", DateTime.Now);
        }

        [Benchmark]
        public void RunUdpWithSampling()
        {
            _udpSender.Increment(2, 0.5, "increment.ud");
        }

        [Benchmark]
        public void RunIp()
        {
            _ipSender.Increment("increment.ip");
            _ipSender.Timing(Timed, "timer.ip");
            _udpSender.Gauge(long.MaxValue / 2, "timer.ip", DateTime.Now);
        }

        [Benchmark]
        public void RunIpWithSampling()
        {
            _ipSender.Increment(2, 0.5, "increment.ip");
        }
    }
}
