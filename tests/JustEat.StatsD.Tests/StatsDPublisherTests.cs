using System;
using Moq;
using Xunit;

namespace JustEat.StatsD
{
    public static class StatsDPublisherTests
    {
        [Fact]
        public static void Decrement_Sends_Multiple_Metrics()
        {
            // Arrange
            var mock = new Mock<IStatsDTransport>();

            var config = new StatsDConfiguration
            {
                Prefix = "red",
            };

            var publisher = new StatsDPublisher(config, mock.Object);

            // Act
            publisher.Decrement(10, 1, "white", "blue");

            // Assert
            mock.Verify((p) => p.Send(It.Ref<ArraySegment<byte>>.IsAny), Times.Exactly(2));
        }

        [Fact]
        public static void Increment_Sends_Multiple_Metrics()
        {
            // Arrange
            var mock = new Mock<IStatsDTransport>();

            var config = new StatsDConfiguration
            {
                Prefix = "red",
            };

            var publisher = new StatsDPublisher(config, mock.Object);

            // Act
            publisher.Increment(10, 1, "white", "blue");

            // Assert
            mock.Verify((p) => p.Send(It.Ref<ArraySegment<byte>>.IsAny), Times.Exactly(2));
        }

        [Fact]
        public static void Metrics_Not_Sent_If_Array_Is_Null_Or_Empty()
        {
            // Arrange
            var mock = new Mock<IStatsDTransport>();
            var config = new StatsDConfiguration();

            var publisher = new StatsDPublisher(config, mock.Object);

            // Act
            publisher.Decrement(1, 1, null as string[]);
            publisher.Increment(1, 1, null as string[]);
            publisher.Decrement(1, 1, Array.Empty<string>());
            publisher.Increment(1, 1, Array.Empty<string>());
            publisher.Decrement(1, 1, new[] { string.Empty });
            publisher.Increment(1, 1, new[] { string.Empty });

            // Assert
            mock.Verify((p) => p.Send(It.IsAny<ArraySegment<byte>>()), Times.Never());
        }

        [Fact]
        public static void Metrics_Not_Sent_If_No_Metrics()
        {
            // Arrange
            var mock = new Mock<IStatsDTransport>();
            var config = new StatsDConfiguration();

            var publisher = new StatsDPublisher(config, mock.Object);

            // Act
            publisher.Decrement(1, 0, new[] { "foo" });
            publisher.Increment(1, 0, new[] { "bar" });

            // Assert
            mock.Verify((p) => p.Send(It.IsAny<ArraySegment<byte>>()), Times.Never());
        }

        [Fact]
        public static void Constructor_Throws_If_Configuration_Is_Null()
        {
            // Arrange
            StatsDConfiguration configuration = null;
            var transport = Mock.Of<IStatsDTransport>();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(
                "configuration",
                () => new StatsDPublisher(configuration, transport));
        }

        [Fact]
        public static void Constructor_Throws_If_Transport_Is_Null()
        {
            // Arrange
            var configuration = new StatsDConfiguration();
            IStatsDTransport transport = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(
                "transport",
                () => new StatsDPublisher(configuration, transport));
        }
    }
}
