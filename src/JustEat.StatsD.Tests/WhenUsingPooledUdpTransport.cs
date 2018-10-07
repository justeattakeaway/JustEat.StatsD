using System;
using System.Net;
using System.Threading.Tasks;
using JustEat.StatsD.EndpointLookups;
using Xunit;

namespace JustEat.StatsD
{
    [Collection("ActiveUdpListeners")]
    public class WhenUsingPooledUdpTransport 
    {
        public WhenUsingPooledUdpTransport(UdpListeners _)
        {
        }

        [Fact]
        public void AMetricCanBeSentWithoutAnExceptionBeingThrown()
        {
            // Arrange
            var endPointSource = EndpointParser.MakeEndPointSource(
                UdpListeners.EndpointA,
                null);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                // Act and Assert
                target.Send("mycustommetric:1|c");
            }
        }

        [Fact]
        public void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownSerial()
        {
            // Arrange
            var endPointSource = EndpointParser.MakeEndPointSource(
                UdpListeners.EndpointA,
                null);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                for (int i = 0; i < 10_000; i++)
                {
                    // Act and Assert
                    target.Send("mycustommetric:1|c");
                }
            }
        }

        [Fact]
        public void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownParallel()
        {
            // Arrange
            var endPointSource = EndpointParser.MakeEndPointSource(
                UdpListeners.EndpointA,
                null);

            using (var target = new PooledUdpTransport(endPointSource))
            {
                Parallel.For(0, 10_000, _ =>
                {
                    // Act and Assert
                    target.Send("mycustommetric:1|c");
                });
            }
        }

        [Fact]
        public static void EndpointSwitchShouldNotCauseExceptionsSequential()
        {
            // Arrange
            var endPointSource1 = EndpointParser.MakeEndPointSource(
                UdpListeners.EndpointA,
                null);

            var endPointSource2 = EndpointParser.MakeEndPointSource(
                UdpListeners.EndpointB,
                null);
            
            using (var target = new PooledUdpTransport(new MilisecondSwitcher(endPointSource2, endPointSource1)))
            {
                for (int i = 0; i < 10_000; i++)
                {
                    // Act and Assert
                    target.Send("mycustommetric:1|c");
                }
            }
        }

        [Fact]
        public static void EndpointSwitchShouldNotCauseExceptionsParallel()
        {
            // Arrange
            var endPointSource1 = EndpointParser.MakeEndPointSource(
                UdpListeners.EndpointA,
                null);

            var endPointSource2 = EndpointParser.MakeEndPointSource(
                UdpListeners.EndpointB,
                null);
            
            using (var target = new PooledUdpTransport(new MilisecondSwitcher(endPointSource2, endPointSource1)))
            {
                Parallel.For(0, 10_000, _ =>
                {
                    // Act and Assert
                    target.Send("mycustommetric:1|c");
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
    }
}
