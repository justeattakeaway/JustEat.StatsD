using System;
using System.Net;
using BenchmarkDotNet.Attributes;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    [MemoryDiagnoser]
    public class UdpTransportBenchmark
    {
        private const string MetricName = "this.is.a.metric:1|c";

        private SocketTransport? _transport;
        private SocketTransport? _transportSwitched;

        private class MillisecondSwitcher : IEndPointSource
        {
            private readonly IEndPointSource _endpointSource1;
            private readonly IEndPointSource _endpointSource2;

            public MillisecondSwitcher(IEndPointSource endpointSource1, IEndPointSource endpointSource2)
            {
                _endpointSource1 = endpointSource1;
                _endpointSource2 = endpointSource2;
            }

            public EndPoint GetEndpoint()
            {
                return DateTime.Now.Millisecond % 2 == 0 ?
                    _endpointSource1.GetEndpoint() :
                    _endpointSource2.GetEndpoint();
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            var config = new StatsDConfiguration
            {
                Host = "127.0.0.1",
            };

            var endpointSource1 = EndPointFactory.MakeEndPointSource(
                config.Host,
                config.Port,
                config.DnsLookupInterval);

            var endpointSource2 = EndPointFactory.MakeEndPointSource(
                config.Host,
                config.Port + 1,
                config.DnsLookupInterval);

            var switcher = new MillisecondSwitcher(endpointSource1, endpointSource2);

            _transport = new SocketTransport(endpointSource1, SocketProtocol.Udp);
            _transportSwitched = new SocketTransport(switcher, SocketProtocol.Udp);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _transport?.Dispose();
            _transportSwitched?.Dispose();
        }

        [Benchmark]
        public void Send()
        {
            _transport!.Send(MetricName);
        }

        [Benchmark]
        public void SendWithSwitcher()
        {
            _transportSwitched!.Send(MetricName);
        }
    }
}
