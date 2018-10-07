using System;
using Moq;
using Xunit;

namespace JustEat.StatsD
{
    public static class StringBasedStatsDPublisherTests
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

            var publisher = new StringBasedStatsDPublisher(config, mock.Object);

            // Act
            publisher.Decrement(10, 1, "white", "blue");

            // Assert
            mock.Verify((p) => p.Send("red.white:-10|c"), Times.Once());
            mock.Verify((p) => p.Send("red.blue:-10|c"), Times.Once());
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

            var publisher = new StringBasedStatsDPublisher(config, mock.Object);

            // Act
            publisher.Increment(10, 1, "white", "blue");

            // Assert
            mock.Verify((p) => p.Send("red.white:10|c"), Times.Once());
            mock.Verify((p) => p.Send("red.blue:10|c"), Times.Once());
        }

        [Fact]
        public static void Metrics_Not_Sent_If_Array_Is_Null_Or_Empty()
        {
            // Arrange
            var mock = new Mock<IStatsDTransport>();
            var config = new StatsDConfiguration();

            var publisher = new StringBasedStatsDPublisher(config, mock.Object);

            // Act
            publisher.Decrement(1, 1, null as string[]);
            publisher.Increment(1, 1, null as string[]);
            publisher.Decrement(1, 1, Array.Empty<string>());
            publisher.Increment(1, 1, Array.Empty<string>());
            publisher.Decrement(1, 1, new[] { string.Empty });
            publisher.Increment(1, 1, new[] { string.Empty });

            // Assert
            mock.Verify((p) => p.Send(It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public static void Metrics_Not_Sent_If_No_Metrics()
        {
            // Arrange
            var mock = new Mock<IStatsDTransport>();
            var config = new StatsDConfiguration();

            var publisher = new StringBasedStatsDPublisher(config, mock.Object);

            // Act
            publisher.Decrement(1, 0, new[] { "foo" });
            publisher.Increment(1, 0, new[] { "bar" });

            // Assert
            mock.Verify((p) => p.Send(It.IsAny<string>()), Times.Never());
        }
    }
}
