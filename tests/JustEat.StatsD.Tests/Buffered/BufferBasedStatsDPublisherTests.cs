using NSubstitute;

namespace JustEat.StatsD.Buffered;

public static class BufferBasedStatsDPublisherTests
{
    [Fact]
    public static void Increment_Is_Noop_If_Bucket_Is_Null()
    {
        // Arrange
        var configuration = new StatsDConfiguration();
        var transport = Substitute.For<IStatsDTransport>();

        var publisher = new BufferBasedStatsDPublisher(configuration, transport);

        // Act
        publisher.Increment(1, 1, null!, null);

        // Assert
        transport.DidNotReceiveWithAnyArgs().Send(default);
    }

    [Fact]
    public static void Increment_Sends_If_Default_Buffer_Is_Too_Small()
    {
        // Arrange
        var configuration = new StatsDConfiguration() { Prefix = new string('a', 513) };
        var transport = Substitute.For<IStatsDTransport>();

        var publisher = new BufferBasedStatsDPublisher(configuration, transport);

        // Act
        publisher.Increment(1, 1, "foo");

        // Assert
        transport.ReceivedWithAnyArgs(1).Send(default);
    }
}
