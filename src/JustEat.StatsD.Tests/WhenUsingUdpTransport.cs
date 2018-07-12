using System.Text;
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
            var target = new UdpTransport(endPointSource);

            // Act and Assert
            target.Send(Encoding.UTF8.GetBytes("mycustommetric"));
        }
    }
}
