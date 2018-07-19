using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark
{
    internal static class Program
    {
        internal static void Main(string[] args)
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
                config.Port + 2,
                config.DnsLookupInterval);

            var wtf = new MilisecondSwitcher(endpointSource1, endpointSource2);

            var pooledTransport = new PooledUdpTransport(wtf);

            Parallel.For(0, 10000, _ => pooledTransport.Send("hello.world.yo"));

            pooledTransport.Dispose();

            //BenchmarkRunner.Run<StatSendingBenchmark>();
            //BenchmarkRunner.Run<FormatterBenchmark>();
            BenchmarkRunner.Run<UdpTransportBenchmark>();
            //BenchmarkRunner.Run<UdpStatSendingBenchmark>();
        }
    }
}
