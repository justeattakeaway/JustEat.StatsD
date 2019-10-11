using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace JustEat.StatsD
{
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
            var transport = Mock.Of<IStatsDTransport>();
            string? metric = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("metric", () => transport.Send(metric!));
        }

        [Fact]
        public static void SendThrowsIfMetricsIsNull()
        {
            // Arrange
            var transport = Mock.Of<IStatsDTransport>();
            IEnumerable<string>? metrics = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("metrics", () => transport.Send(metrics!));
        }

        [Fact]
        public static void SendSendsStringMetric()
        {
            // Arrange
            var transport = new Mock<IStatsDTransport>();
            string metric = "metric";

            // Act
            transport.Object.Send(metric);

            // Assert
            transport.Verify((p) => p.Send(It.Ref<ArraySegment<byte>>.IsAny), Times.Once());
        }

        [Fact]
        public static void SendSendsStringMetrics()
        {
            // Arrange
            var transport = new Mock<IStatsDTransport>();
            var metrics = new[] { "a", "b", "c" };

            // Act
            transport.Object.Send(metrics);

            // Assert
            transport.Verify((p) => p.Send(It.Ref<ArraySegment<byte>>.IsAny), Times.Exactly(3));
        }
    }
}
