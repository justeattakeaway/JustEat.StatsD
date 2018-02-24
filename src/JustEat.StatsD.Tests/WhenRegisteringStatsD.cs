#if !NET451
using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public static class WhenRegisteringStatsD
    {
        [Fact]
        public static void CanRegisterServicesWithNoConfiguratonIfConfigurationAlreadRegistered()
        {
            // Arrange
            var config = new StatsDConfiguration()
            {
                Host = "localhost"
            };

            var provider = Configure(
                (services) =>
                {
                    services.AddSingleton(config);

                    // Act
                    services.AddStatsD();
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldNotBeNull();
            configuration.ShouldBe(config);

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<UdpTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
        }

        [Fact]
        public static void CanRegisterServicesWithAHost()
        {
            // Arrange
            string host = "localhost";

            var provider = Configure(
                (services) =>
                {
                    // Act
                    services.AddStatsD(host);
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldNotBeNull();
            configuration.Host.ShouldBe(host);
            configuration.Prefix.ShouldBeEmpty();

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<UdpTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
        }

        [Fact]
        public static void CanRegisterServicesWithAHostAndPrefix()
        {
            // Arrange
            string host = "localhost";
            string prefix = "myprefix";

            var provider = Configure(
                (services) =>
                {
                    // Act
                    services.AddStatsD(host, prefix);
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldNotBeNull();
            configuration.Host.ShouldBe(host);
            configuration.Prefix.ShouldBe(prefix);

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<UdpTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
        }

        [Fact]
        public static void CanRegisterServicesWithConfigurationObject()
        {
            // Arrange
            var config = new StatsDConfiguration()
            {
                Host = "localhost"
            };

            var provider = Configure(
                (services) =>
                {
                    // Act
                    services.AddStatsD(config);
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldBe(config);

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<UdpTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
        }

        [Fact]
        public static void CanRegisterServicesWithConfigurationAction()
        {
            // Arrange
            var provider = Configure(
                (services) =>
                {
                    // Act
                    services.AddStatsD((config) => config.Host = "localhost");
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldNotBeNull();
            configuration.Host.ShouldBe("localhost");
            configuration.Prefix.ShouldBeEmpty();

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<UdpTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
        }

        [Fact]
        public static void CanRegisterServicesWithConfigurationActionWithServiceProvider()
        {
            // Arrange
            var options = new MyOptions()
            {
                StatsDHost = "localhost"
            };

            var provider = Configure(
                (services) =>
                {
                    services.AddSingleton(options);

                    // Act
                    services.AddStatsD(
                        (serviceProvider, config) =>
                        {
                            var myOptions = serviceProvider.GetRequiredService<MyOptions>();

                            config.Host = myOptions.StatsDHost;
                        });
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldNotBeNull();
            configuration.Host.ShouldBe(options.StatsDHost);
            configuration.Prefix.ShouldBeEmpty();

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<UdpTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
        }

        [Fact]
        public static void CanRegisterServicesWithFactoryMethod()
        {
            // Arrange
            var options = new MyOptions()
            {
                StatsDHost = "localhost"
            };

            var provider = Configure(
                (services) =>
                {
                    services.AddSingleton(options);

                    // Act
                    services.AddStatsD(
                        (serviceProvider) =>
                        {
                            var myOptions = serviceProvider.GetRequiredService<MyOptions>();

                            return new StatsDConfiguration()
                            {
                                Host = myOptions.StatsDHost
                            };
                        });
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldNotBeNull();
            configuration.Host.ShouldBe(options.StatsDHost);
            configuration.Prefix.ShouldBeEmpty();

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<UdpTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
        }

        [Fact]
        public static void RegisteringServicesDoesNotOverwriteExistingRegistrations()
        {
            // Arrange
            var existingConfig = new StatsDConfiguration();
            var existingTransport = Mock.Of<IStatsDTransport>();
            var existingPublisher = Mock.Of<IStatsDPublisher>();

            var provider = Configure(
                (services) =>
                {
                    services.AddSingleton(existingConfig);
                    services.AddSingleton(existingTransport);
                    services.AddSingleton(existingPublisher);

                    // Act
                    services.AddStatsD();
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldBe(existingConfig);

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldBe(existingTransport);

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldBe(existingPublisher);
        }

        [Fact]
        public static void CanRegisterServicesWithoutFirstConfiguringTheHost()
        {
            // Act and Assert
            Should.NotThrow(() => Configure((services) => services.AddStatsD()));
        }

        [Fact]
        public static void ParametersAreCheckedForNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            StatsDConfiguration configuration = null;
            Action<StatsDConfiguration> configure = null;
            Action<IServiceProvider, StatsDConfiguration> configureWithProvider = null;
            Func<IServiceProvider, StatsDConfiguration> configurationFactory = null;
            string host = null;

            // Act and Assert
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configuration)).ParamName.ShouldBe("configuration");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configure)).ParamName.ShouldBe("configure");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configureWithProvider)).ParamName.ShouldBe("configure");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configurationFactory)).ParamName.ShouldBe("configurationFactory");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(host)).ParamName.ShouldBe("host");

            // Arrange
            services = null;

            // Act and Assert
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configuration)).ParamName.ShouldBe("services");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configure)).ParamName.ShouldBe("services");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configureWithProvider)).ParamName.ShouldBe("services");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configurationFactory)).ParamName.ShouldBe("services");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(host)).ParamName.ShouldBe("services");
        }

        private static IServiceProvider Configure(Action<IServiceCollection> registration)
        {
            var services = new ServiceCollection();

            registration(services);

            return services.BuildServiceProvider();
        }

        private sealed class MyOptions
        {
            public string StatsDHost { get; set; }
        }
    }
}
#endif
