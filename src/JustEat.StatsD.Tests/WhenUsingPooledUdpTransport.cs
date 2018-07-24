using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JustEat.StatsD.EndpointLookups;
using Xunit;

namespace JustEat.StatsD
{
    public static class WhenUsingPooledUdpTransport
    {
        [Fact]
        public static void AMetricCanBeSentWithoutAnExceptionBeingThrown()
        {
            // Arrange
            var endPointSource = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                // Act and Assert
                target.Send("mycustommetric");
            }
        }

        [Fact]
        public static void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownSerial()
        {
            // Arrange
            var endPointSource = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                for (int i = 0; i < 10_000; i++)
                {
                    // Act and Assert
                    target.Send("mycustommetric");
                }
            }
        }

        [Fact]
        public static void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownParallel()
        {
            // Arrange
            var endPointSource = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                Parallel.For(0, 10_000, _ =>
                {
                    target.Send("mycustommetric");
                });
            }
        }

        private class MilisecondSwitcher : IPEndPointSource
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

        [Fact]
        public static void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownParallel_WithChangingEndpoints()
        {
            // Arrange
            var endPointSource1 = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);
            var endPointSource2 = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8126, null);

            var endPointSource = new MilisecondSwitcher(endPointSource1, endPointSource2);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                Parallel.For(0, 100_000, _ =>
                {
                    target.Send("mycustommetric");
                });
            }

            Thread.Sleep(5000);
        }
    }
}
