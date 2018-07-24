using System;
using System.Net;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    public class MilisecondSwitcher : IPEndPointSource
    {
        private readonly IPEndPointSource _endpointSource1;
        private readonly IPEndPointSource _endpointSource2;

        public MilisecondSwitcher(IPEndPointSource endpointSource1, IPEndPointSource endpointSource2)
        {
            _endpointSource1 = endpointSource1;
            _endpointSource2 = endpointSource2;
        }

        public IPEndPoint GetEndpoint()
        {
            return DateTime.Now.Millisecond % 2 == 0 ?
                _endpointSource1.GetEndpoint() :
                _endpointSource2.GetEndpoint();
        }
    }

    [MemoryDiagnoser]
    public class UdpTransportBenchmark
    {
        private const string MetricName = "this.is.a.metric";

        private PooledUdpTransport _pooledTransport;
        private PooledUdpTransport _pooledTransportSwitched;
        private UdpTransport _unpooledTransport;

        [GlobalSetup]
        public void Setup()
        {
            var config = new StatsDConfiguration
            {
                Host = "127.0.0.1",
            };

            var endpointSource1 = EndpointParser.MakeEndPointSource(
                config.Host,
                config.Port,
                config.DnsLookupInterval);

            var endpointSource2 = EndpointParser.MakeEndPointSource(
                config.Host,
                config.Port + 1,
                config.DnsLookupInterval);

            var switcher = new MilisecondSwitcher(endpointSource1, endpointSource2);

            _pooledTransport = new PooledUdpTransport(endpointSource1);
            _pooledTransportSwitched = new PooledUdpTransport(switcher);
            _unpooledTransport = new UdpTransport(endpointSource1);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _pooledTransport?.Dispose();
            _pooledTransportSwitched?.Dispose();
        }

        [Benchmark(Baseline = true)]
        public void Send()
        {
            _unpooledTransport.Send(MetricName);
        }

        [Benchmark]
        public void SendWithPool()
        {
            _pooledTransport.Send(MetricName);
        }

        //[Benchmark]
        //public void SendWithPoolSwitcher()
        //{
        //    _pooledTransportSwitched.Send(MetricName);
        //}
    }
}
