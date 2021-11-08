using Moq;

namespace JustEat.StatsD.Buffered
{
    public static class BufferBasedStatsDPublisherTests
    {
        [Fact]
        public static void Increment_Is_Noop_If_Bucket_Is_Null()
        {
            // Arrange
            var configuration = new StatsDConfiguration();
            var transport = new Mock<IStatsDTransport>();

            var publisher = new BufferBasedStatsDPublisher(configuration, transport.Object);

            // Act
            publisher.Increment(1, 1, null!, null);

            // Assert
            transport.Verify((p) => p.Send(It.Ref<ArraySegment<byte>>.IsAny), Times.Never());
        }

        [Fact]
        public static void Increment_Sends_If_Default_Buffer_Is_Too_Small()
        {
            // Arrange
            var configuration = new StatsDConfiguration() { Prefix = new string('a', 513) };
            var transport = new Mock<IStatsDTransport>();

            var publisher = new BufferBasedStatsDPublisher(configuration, transport.Object);

            // Act
            publisher.Increment(1, 1, "foo");

            // Assert
            transport.Verify((p) => p.Send(It.Ref<ArraySegment<byte>>.IsAny), Times.Once());
        }
    }
}
