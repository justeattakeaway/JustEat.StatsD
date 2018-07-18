using System;
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

            var stats = new StatsDPublisher(validConfig, Mock.Of<IStatsDTransport>());

            stats.ShouldNotBeNull();
        }

        [Fact]
        public void ConfigurationIsValidWithHostIp()
        {
            var validConfig = new StatsDConfiguration
            {
                Host = "10.0.1.2"
            };

            var stats = new StatsDPublisher(validConfig, Mock.Of<IStatsDTransport>());

            stats.ShouldNotBeNull();
        }

        [Fact]
        public void ConfigurationIsNull()
        {
            StatsDConfiguration noConfig = null;

            Should.Throw<ArgumentNullException>(
             () => new StatsDPublisher(noConfig, Mock.Of<IStatsDTransport>()));
        }
    }
}
