using NSubstitute;

namespace JustEat.StatsD;

public static class IStatsDTransportExtensionsTests
{
    [Fact]
    public static void SendThrowsIfTransportIsNullMetric()
    {
        // Arrange
        IStatsDTransport? transport = null;
        string metric = "metric";

        // Act and Assert
        Assert.Throws<ArgumentNullException>("transport", () => transport!.Send(metric));
    }

    [Fact]
    public static void SendThrowsIfTransportIsNullMetrics()
    {
        // Arrange
        IStatsDTransport? transport = null;
        IEnumerable<string> metrics = Array.Empty<string>();

        // Act and Assert
        Assert.Throws<ArgumentNullException>("transport", () => transport!.Send(metrics));
    }

    [Fact]
    public static void SendThrowsIfTransportIfMetricIsNull()
    {
        // Arrange
        var transport = Substitute.For<IStatsDTransport>();
        string? metric = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>("metric", () => transport.Send(metric!));
    }

    [Fact]
    public static void SendThrowsIfMetricsIsNull()
    {
        // Arrange
        var transport = Substitute.For<IStatsDTransport>();
        IEnumerable<string>? metrics = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>("metrics", () => transport.Send(metrics!));
    }

    [Fact]
    public static void SendSendsStringMetric()
    {
        // Arrange
        var transport = Substitute.For<IStatsDTransport>();
        string metric = "metric";

        // Act
        transport.Send(metric);

        // Assert
        transport.ReceivedWithAnyArgs(1).Send(default);
    }

    [Fact]
    public static void SendSendsStringMetrics()
    {
        // Arrange
        var transport = Substitute.For<IStatsDTransport>();
        var metrics = new[] { "a", "b", "c" };

        // Act
        transport.Send(metrics);

        // Assert
        transport.ReceivedWithAnyArgs(3).Send(default);
    }
}
