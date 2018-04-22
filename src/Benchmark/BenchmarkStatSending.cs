using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class BenchmarkStatSending
    {
        private IStatsDPublisher _udpSender;
        private IStatsDPublisher _ipSender;

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
            _udpSender.Increment("startup.udp");

            _ipSender = new StatsDPublisher(config, ipTransport);
            _udpSender.Increment("startup.ip");
        }

        [Benchmark]
        public void RunUdp()
        {
            _udpSender.Increment("increment.udp");
            _udpSender.Timing(TimeSpan.FromMinutes(1), "timer.udp");
        }

        [Benchmark]
        public void RunIp()
        {
            _ipSender.Increment("increment.ip");
            _ipSender.Timing(TimeSpan.FromMinutes(1), "timer.ip");
        }
    }
}
