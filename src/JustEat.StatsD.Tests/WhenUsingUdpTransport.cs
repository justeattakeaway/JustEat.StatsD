using Xunit;

namespace JustEat.StatsD
{
    public static class WhenUsingUdpTransport
    {
        [Fact]
        public static void AMetricCanBeSentWithoutAnExceptionBeingThrown()
        {
            // Arrange
            var endPointSource = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);

            using (var target = new UdpTransport(endPointSource))
            {
                // Act and Assert
                target.Send("mycustommetric");
            }
        }

        [Fact]
        public static void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrown()
        {
            // Arrange
            var endPointSource = EndpointLookups.EndpointParser.MakeEndPointSource("127.0.0.1", 8125, null);

            using (var target = new UdpTransport(endPointSource))
            {
                for (int i = 0; i < 10_000; i++)
                {
                    // Act and Assert
                    target.Send("mycustommetric");
                }
            }
        }
    }
}
