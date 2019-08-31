using System;
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

            using (new StatsDPublisher(validConfig))
            {
            }
        }

        [Fact]
        public void ConfigurationIsValidWithHostIp()
        {
            var validConfig = new StatsDConfiguration
            {
                Host = "10.0.1.2"
            };

            using var stats = new StatsDPublisher(validConfig);
        }

        [Fact]
        public void ConfigurationIsNull()
        {
            StatsDConfiguration? configuration = null;

            Assert.Throws<ArgumentNullException>(
                "configuration",
                () => new StatsDPublisher(configuration!));
        }

        [Fact]
        public void ConfigurationHasNoHost()
        {
            var configuration = new StatsDConfiguration
            {
                Host = null
            };

            Assert.Throws<ArgumentException>(
                "configuration",
                () => new StatsDPublisher(configuration));
        }
    }
}
