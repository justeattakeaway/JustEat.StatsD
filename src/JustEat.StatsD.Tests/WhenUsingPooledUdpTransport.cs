using System.Threading.Tasks;
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
    }
}
