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
        private BufferBasedStatsDPublisher _ipSender;

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

            var endpointSource = EndPointFactory.MakeEndPointSource(
                config.Host, config.Port, config.DnsLookupInterval);

            var ipTransport = new SocketTransport(endpointSource, SocketProtocol.IP);
            _ipSender = new BufferBasedStatsDPublisher(config, ipTransport);
            _ipSender.Increment("startup.i");

            var udpTransport = new SocketTransport(endpointSource, SocketProtocol.Udp);
            _udpSender = new BufferBasedStatsDPublisher(config, udpTransport);
            _udpSender.Increment("startup.u");
        }


        [Benchmark]
        public void RunIp()
        {
            _ipSender.Increment("hello.i");
            _ipSender.Increment(20, "increment.i");
            _ipSender.Timing(Timed, "timer.i");
            _ipSender.Gauge(354654, "gauge.i");
            _ipSender.Gauge(25.1, "free-space.i");
        }

        [Benchmark]
        public void RunIPWithSampling()
        {
            _ipSender.Increment(2, 0.2, "increment.i");
            _ipSender.Timing(2, 0.2, "increment.i");
        }

        [Benchmark]
        public void RunUdp()
        {
            _udpSender.Increment("hello.u");
            _udpSender.Increment(20, "increment.u");
            _udpSender.Timing(Timed, "timer.u");
            _udpSender.Gauge(354654, "gauge.u");
            _udpSender.Gauge(25.1, "free-space.u");
        }

        [Benchmark]
        public void RunUdpWithSampling()
        {
            _udpSender.Increment(2, 0.2, "increment.u");
            _udpSender.Timing(2, 0.2, "increment.u");
        }
    }
}
