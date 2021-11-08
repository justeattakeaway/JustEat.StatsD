using System.Net;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD;

[Collection("ActiveUdpListeners")]
public class WhenUsingUdpTransport
{

#pragma warning disable CA1801 // Used to force the creation of the collection
    public WhenUsingUdpTransport(UdpListeners _)
    {
    }
#pragma warning restore CA1801

    [Fact]
    public void AMetricCanBeSentWithoutAnExceptionBeingThrown()
    {
        // Arrange
        var endPointSource = EndPointFactory.MakeEndPointSource(
            UdpListeners.EndpointA,
            null);

        using var target = new SocketTransport(endPointSource, SocketProtocol.Udp);

        // Act and Assert
        target.Send("mycustommetric");
    }

    [Fact]
    public void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownSerial()
    {
        // Arrange
        var endPointSource = EndPointFactory.MakeEndPointSource(
            UdpListeners.EndpointA,
            null);

        using var target = new SocketTransport(endPointSource, SocketProtocol.Udp);

        for (int i = 0; i < 10_000; i++)
        {
            // Act and Assert
            target.Send("mycustommetric:1|c");
        }
    }

    [Fact]
    public void MultipleMetricsCanBeSentWithoutAnExceptionBeingThrownParallel()
    {
        // Arrange
        var endPointSource = EndPointFactory.MakeEndPointSource(
            UdpListeners.EndpointA,
            null);

        using var target = new SocketTransport(endPointSource, SocketProtocol.Udp);

        Parallel.For(0, 10_000, _ =>
            {
                // Act and Assert
                target.Send("mycustommetric:1|c");
            });
    }

    [Fact]
    public static void EndpointSwitchShouldNotCauseExceptionsSequential()
    {
        // Arrange
        var endPointSource1 = EndPointFactory.MakeEndPointSource(
            UdpListeners.EndpointA,
            null);

        var endPointSource2 = EndPointFactory.MakeEndPointSource(
            UdpListeners.EndpointB,
            null);

        using var target = new SocketTransport(new MillisecondSwitcher(endPointSource2, endPointSource1), SocketProtocol.Udp);

        for (int i = 0; i < 10_000; i++)
        {
            // Act and Assert
            target.Send("mycustommetric:1|c");
        }
    }

    [Fact]
    public static void EndpointSwitchShouldNotCauseExceptionsParallel()
    {
        // Arrange
        var endPointSource1 = EndPointFactory.MakeEndPointSource(
            UdpListeners.EndpointA,
            null);

        var endPointSource2 = EndPointFactory.MakeEndPointSource(
            UdpListeners.EndpointB,
            null);

        using var target = new SocketTransport(new MillisecondSwitcher(endPointSource2, endPointSource1), SocketProtocol.Udp);

        Parallel.For(0, 10_000, _ =>
            {
                // Act and Assert
                target.Send("mycustommetric");
            });
    }

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
}
