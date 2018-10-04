#if !NET451
using System;
using System.Collections.Generic;
using JustEat.StatsD.EndpointLookups;
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

            try
            {
                // Assert
                var configuration = provider.GetRequiredService<StatsDConfiguration>();
                configuration.ShouldNotBeNull();
                configuration.ShouldBe(config);

                var source = provider.GetRequiredService<IPEndPointSource>();
                source.ShouldNotBeNull();

                var transport = provider.GetRequiredService<IStatsDTransport>();
                transport.ShouldNotBeNull();
                transport.ShouldBeOfType<UdpTransport>();

                var publisher = provider.GetRequiredService<IStatsDPublisher>();
                publisher.ShouldNotBeNull();
                publisher.ShouldBeOfType<StatsDPublisher>();
            }
            finally
            {
                if (provider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
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

            try
            {
                // Assert
                var configuration = provider.GetRequiredService<StatsDConfiguration>();
                configuration.ShouldNotBeNull();
                configuration.Host.ShouldBe(host);
                configuration.Prefix.ShouldBeEmpty();

                var source = provider.GetRequiredService<IPEndPointSource>();
                source.ShouldNotBeNull();

                var transport = provider.GetRequiredService<IStatsDTransport>();
                transport.ShouldNotBeNull();
                transport.ShouldBeOfType<UdpTransport>();

                var publisher = provider.GetRequiredService<IStatsDPublisher>();
                publisher.ShouldNotBeNull();
                publisher.ShouldBeOfType<StatsDPublisher>();
            }
            finally
            {
                if (provider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
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

            try
            {
                // Assert
                var configuration = provider.GetRequiredService<StatsDConfiguration>();
                configuration.ShouldNotBeNull();
                configuration.Host.ShouldBe(host);
                configuration.Prefix.ShouldBe(prefix);

                var source = provider.GetRequiredService<IPEndPointSource>();
                source.ShouldNotBeNull();

                var transport = provider.GetRequiredService<IStatsDTransport>();
                transport.ShouldNotBeNull();
                transport.ShouldBeOfType<UdpTransport>();

                var publisher = provider.GetRequiredService<IStatsDPublisher>();
                publisher.ShouldNotBeNull();
                publisher.ShouldBeOfType<StatsDPublisher>();
            }
            finally
            {
                if (provider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
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

            try
            {
                // Assert
                var configuration = provider.GetRequiredService<StatsDConfiguration>();
                configuration.ShouldNotBeNull();
                configuration.Host.ShouldBe(options.StatsDHost);
                configuration.Prefix.ShouldBeEmpty();

                var source = provider.GetRequiredService<IPEndPointSource>();
                source.ShouldNotBeNull();

                var transport = provider.GetRequiredService<IStatsDTransport>();
                transport.ShouldNotBeNull();
                transport.ShouldBeOfType<UdpTransport>();

                var publisher = provider.GetRequiredService<IStatsDPublisher>();
                publisher.ShouldNotBeNull();
                publisher.ShouldBeOfType<StatsDPublisher>();
            }
            finally
            {
                if (provider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        [Fact]
        public static void RegisteringServicesDoesNotOverwriteExistingRegistrations()
        {
            // Arrange
            var existingConfig = new StatsDConfiguration();
            var existingSource = Mock.Of<IPEndPointSource>();
            var existingTransport = Mock.Of<IStatsDTransport>();
            var existingPublisher = Mock.Of<IStatsDPublisher>();

            var provider = Configure(
                (services) =>
                {
                    services.AddSingleton(existingConfig);
                    services.AddSingleton(existingSource);
                    services.AddSingleton(existingTransport);
                    services.AddSingleton(existingPublisher);

                    // Act
                    services.AddStatsD();
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldBe(existingConfig);

            var source = provider.GetRequiredService<IPEndPointSource>();
            source.ShouldBe(existingSource);

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
            Func<IServiceProvider, StatsDConfiguration> configurationFactory = null;
            string host = null;

            // Act and Assert
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configurationFactory)).ParamName.ShouldBe("configurationFactory");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(host)).ParamName.ShouldBe("host");

            // Arrange
            services = null;

            // Act and Assert
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(configurationFactory)).ParamName.ShouldBe("services");
            Should.Throw<ArgumentNullException>(() => services.AddStatsD(host)).ParamName.ShouldBe("services");
        }

        [Fact]
        public static void CanRegisterServicesWithCustomTransport()
        {
            // Arrange
            string host = "localhost";

            var provider = Configure(
                (services) =>
                {
                    services.AddSingleton<IStatsDTransport, MyTransport>();

                    // Act
                    services.AddStatsD(host);
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldNotBeNull();
            configuration.Host.ShouldBe(host);
            configuration.Prefix.ShouldBeEmpty();

            var source = provider.GetRequiredService<IPEndPointSource>();
            source.ShouldNotBeNull();

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<MyTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
        }

        [Fact]
        public static void CanRegisterServicesWithIPTransport()
        {
            // Arrange
            string host = "127.0.0.1";

            var provider = Configure(
                (services) =>
                {
                    // Act
                    services.AddSingleton<IStatsDTransport, IpTransport>();
                    services.AddStatsD(host);
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldNotBeNull();
            configuration.Host.ShouldBe(host);
            configuration.Prefix.ShouldBeEmpty();

            var source = provider.GetRequiredService<IPEndPointSource>();
            source.ShouldNotBeNull();

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<IpTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
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

        private sealed class MyTransport : IStatsDTransport
        {
            public MyTransport(IPEndPointSource endpointSource)
            {
            }

            public void Send(string metric)
            {
            }

            public void Send(IEnumerable<string> metrics)
            {
            }
        }
    }
}
#endif
