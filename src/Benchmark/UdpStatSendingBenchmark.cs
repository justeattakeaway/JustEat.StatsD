using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class UdpStatSendingBenchmark
    {
        private static readonly TimeSpan Timed = TimeSpan.FromMinutes(1);

        private SocketTransport _transport;
        private StringBasedStatsDPublisher _udpSender;
        private BufferBasedStatsDPublisher _bufferBasedStatsDPublisher;
        private StatsDPublisher _adaptedStatsDPublisher;

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
                config.Host,
                config.Port,
                config.DnsLookupInterval);

            _transport = new SocketTransport(endpointSource, SocketProtocol.Udp);

            _udpSender = new StringBasedStatsDPublisher(config, _transport);
            _udpSender.Increment("startup.ud");

            _adaptedStatsDPublisher = new StatsDPublisher(config);
            _adaptedStatsDPublisher.Increment("startup.ud");

            _bufferBasedStatsDPublisher = new BufferBasedStatsDPublisher(config, _transport);
            _bufferBasedStatsDPublisher.Increment("startup.ud");
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _transport?.Dispose();
            _adaptedStatsDPublisher?.Dispose();
        }

        [Benchmark]
        public void SendStatUdp()
        {
            _udpSender.Increment("increment.ud");
            _udpSender.Timing(Timed, "timer.ud");
        }

        [Benchmark]
        public void SendStatUdpBuffered()
        {
            _bufferBasedStatsDPublisher.Increment("increment.ud");
            _bufferBasedStatsDPublisher.Timing(Timed, "timer.ud");
        }

        [Benchmark]
        public void SendStatUdpCoveredByAdapter()
        {
            _adaptedStatsDPublisher.Increment("increment.ud");
            _adaptedStatsDPublisher.Timing(Timed, "timer.ud");
        }
    }
}
