using BenchmarkDotNet.Attributes;
using JustEat.StatsD;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class StatSendingBenchmark
    {
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

            var udpTransport = new UdpTransport(endpointSource);
            var ipTransport = new IpTransport(endpointSource);

            _udpSender = new StatsDPublisher(config, udpTransport);
            _udpSender.Increment("startup.ud");

            _ipSender = new StatsDPublisher(config, ipTransport);
            _udpSender.Increment("startup.ip");
        }

        [Benchmark]
        public void RunUdp()
        {
            _udpSender.Increment("increment.ud");
            _udpSender.Timing(Timed, "timer.ud");
        }

        [Benchmark]
        public void RunIp()
        {
            _ipSender.Increment("increment.ip");
            _ipSender.Timing(Timed, "timer.ip");
        }
    }
}
