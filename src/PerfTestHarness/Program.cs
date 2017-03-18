using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JustEat.StatsD;
using JustEat.StatsD.EndpointLookups;

namespace PerfTestHarness
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var iterations = Enumerable.Range(1, 500000);
            var endpoint = EndpointParser.MakeEndPointSource("localhost", 3128, TimeSpan.FromMinutes(1));
            var client = new StatsDUdpTransport(endpoint);
            var formatter = new StatsDMessageFormatter(CultureInfo.InvariantCulture);
            var watch = new Stopwatch();

            Console.WriteLine("To start - hit ENTER.");
            Console.ReadLine();
            Console.WriteLine("start");
            watch.Start();

            Parallel.ForEach(iterations, x => client.Send(formatter.Gauge(x, "bucket_sample" + "number-of-messages-to-be-sent")));

            watch.Stop();
            Console.WriteLine("end - " + watch.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
