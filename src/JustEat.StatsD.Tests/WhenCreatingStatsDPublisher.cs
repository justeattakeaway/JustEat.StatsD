using System;
using JustEat.StatsD.Buffered;
using Moq;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public class WhenCreatingStatsDPublisher
    {
        [Fact]
        public void ConfigurationIsValidWithHostName()
        {
            var validConfig = new StatsDConfiguration
            {
                Host = "someserver.somewhere.com"
            };

            var stats = new StringBasedStatsDPublisher(validConfig);

            stats.ShouldNotBeNull();
        }

        [Fact]
        public void ConfigurationIsValidWithHostIp()
        {
            var validConfig = new StatsDConfiguration
            {
                Host = "10.0.1.2"
            };

            var stats = new StringBasedStatsDPublisher(validConfig);

            stats.ShouldNotBeNull();
        }

        [Fact]
        public void ConfigurationIsNull()
        {
            StatsDConfiguration noConfig = null;

            Should.Throw<ArgumentNullException>(
             () => new StringBasedStatsDPublisher(noConfig));
        }

        [Fact]
        public void ConfigurationHasNoHost()
        {
            var badConfig = new StatsDConfiguration
            {
                Host = null
            };

            Should.Throw<ArgumentNullException>(
             () => new StringBasedStatsDPublisher(badConfig));
        }

        private class BothVersionsTransportMock : IStatsDTransport, IStatsDBufferedTransport
        {
            public int StringCalls { get; private set; }
            public int BufferedCalls { get; private set; }

            public void Send(string metric)
            {
                StringCalls++;
            }

            public void Send(in ArraySegment<byte> metric)
            {
                BufferedCalls++;
            }
        }

        [Fact]
        public void BufferBasedTransportShouldBeChosenIfAvailableAndAskedFor()
        {
            var config = new StatsDConfiguration
            {
                Prefix = "test",
                Host = "127.0.0.1",
                PreferBufferedTransport = true
            };

            var transport = new BothVersionsTransportMock();

            var publisher = new StatsDPublisher(config, transport);

            publisher.MarkEvent("test");

            transport.BufferedCalls.ShouldBe(1);
            transport.StringCalls.ShouldBe(0);
        }

        [Fact]
        public void BufferBasedTransportShouldBeChosenByDefault()
        {
            var config = new StatsDConfiguration
            {
                Prefix = "test",
                Host = "127.0.0.1",
                PreferBufferedTransport = true
            };

            var transport = new BothVersionsTransportMock();

            var publisher = new StatsDPublisher(config, transport);

            publisher.MarkEvent("test");

            transport.BufferedCalls.ShouldBe(1);
            transport.StringCalls.ShouldBe(0);
        }

        [Fact]
        public void StringBasedTransportShouldBeUsedIfBufferedIsNotAvailable()
        {
            var config = new StatsDConfiguration
            {
                Prefix = "test",
                Host = "127.0.0.1",
                PreferBufferedTransport = false
            };

            var transport = new Mock<IStatsDTransport>(MockBehavior.Loose);

            var publisher = new StatsDPublisher(config, transport.Object);

            publisher.MarkEvent("test");

            transport.Verify(x => x.Send(It.IsAny<string>()), Times.Once);
        }
    }
}
