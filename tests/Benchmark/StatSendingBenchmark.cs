using System;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class StatSendingBenchmark : IDisposable
    {
        private bool _disposed;
        private SocketTransport _udpTransport;
        private BufferBasedStatsDPublisher _udpSender;
        private SocketTransport _ipTransport;
        private BufferBasedStatsDPublisher _ipSender;

        private static readonly TimeSpan Timed = TimeSpan.FromMinutes(1);

        ~StatSendingBenchmark()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _udpTransport?.Dispose();
                    _ipTransport?.Dispose();
                }

                _disposed = true;
            }
        }

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

            _ipTransport = new SocketTransport(endpointSource, SocketProtocol.IP);
            _ipSender = new BufferBasedStatsDPublisher(config, _ipTransport);
            _ipSender.Increment("startup.i");

            _udpTransport = new SocketTransport(endpointSource, SocketProtocol.Udp);
            _udpSender = new BufferBasedStatsDPublisher(config, _udpTransport);
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
