using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class StatSendingBenchmark
    {
        private UdpTransport _udpTransport;
        private IStatsDPublisher _udpSender;
        private IStatsDPublisher _ipSender;

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

            _udpTransport = new UdpTransport(endpointSource);
            var ipTransport = new IpTransport(endpointSource);

            _udpSender = new StatsDPublisher(config, _udpTransport);
            _udpSender.Increment("startup.ud");

            _ipSender = new StatsDPublisher(config, ipTransport);
            _udpSender.Increment("startup.ip");
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _udpTransport.Dispose();
        }

        [Benchmark]
        public void RunUdp()
        {
            _udpSender.Increment("increment.ud");
            _udpSender.Timing(Timed, "timer.ud");
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
        }

        [Benchmark]
        public void RunIpWithSampling()
        {
            _ipSender.Increment(2, 0.5, "increment.ip");
        }
    }
}
