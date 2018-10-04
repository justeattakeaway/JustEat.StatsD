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
        private BufferBasedStatsDPublisher _udpSender;
        private StringBasedStatsDPublisher _ipSender;

        private static readonly TimeSpan Timed = TimeSpan.FromMinutes(1);

        [GlobalSetup]
        public void Setup()
        {
            var config = new StatsDConfiguration
            {
                // if you want to verify that stats are received,
                // you will need the IP of suitable local test stats server
                Host = "127.0.0.1",
                Prefix = "testmetric"
            };

            var endpointSource = EndpointParser.MakeEndPointSource(
                config.Host, config.Port, config.DnsLookupInterval);

            var ipTransport = new IpTransport(endpointSource);
            _ipSender = new StringBasedStatsDPublisher(config, ipTransport);
            _ipSender.Increment("startup.ip");

            var udpTransport = new UdpTransport(endpointSource, SocketTransport.Udp);
            _udpSender = new BufferBasedStatsDPublisher(config, udpTransport);
            _udpSender.Increment("startup.v2");
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
            _udpSender.MarkEvent("hello.v2");
            _udpSender.Increment(20, "increment.v2");
            _udpSender.Timing(Timed, "timer.v2");
            _udpSender.Gauge(354654, "gauge.v2");
            _udpSender.Gauge(25.1, "free-space.v2");
        }

        [Benchmark]
        public void RunBufferedWithSampling()
        {
            _udpSender.Increment(2, 0.2, "increment.v2");
            _udpSender.Timing(2, 0.2, "increment.v2");
        }
    }
}
