using System;
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
        public static void CanRegisterServicesWithNoConfigurationIfConfigurationAlreadyRegistered()
        {
            // Arrange
            var config = new StatsDConfiguration
            {
                Host = "localhost"
            };

            var provider = Configure(services =>
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

                var source = provider.GetRequiredService<IEndPointSource>();
                source.ShouldNotBeNull();

                var transport = provider.GetRequiredService<IStatsDTransport>();
                transport.ShouldNotBeNull();
                transport.ShouldBeOfType<SocketTransport>();

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

            var provider = Configure(services =>
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

                var source = provider.GetRequiredService<IEndPointSource>();
                source.ShouldNotBeNull();

                var transport = provider.GetRequiredService<IStatsDTransport>();
                transport.ShouldNotBeNull();
                transport.ShouldBeOfType<SocketTransport>();

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

            var provider = Configure(services =>
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

                var source = provider.GetRequiredService<IEndPointSource>();
                source.ShouldNotBeNull();

                var transport = provider.GetRequiredService<IStatsDTransport>();
                transport.ShouldNotBeNull();
                transport.ShouldBeOfType<SocketTransport>();

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
            var options = new MyOptions
            {
                StatsDHost = "localhost"
            };

            var provider = Configure(services =>
                {
                    services.AddSingleton(options);

                    // Act
                    services.AddStatsD(serviceProvider =>
                        {
                            var myOptions = serviceProvider.GetRequiredService<MyOptions>();

                            return new StatsDConfiguration
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

                var source = provider.GetRequiredService<IEndPointSource>();
                source.ShouldNotBeNull();

                var transport = provider.GetRequiredService<IStatsDTransport>();
                transport.ShouldNotBeNull();
                transport.ShouldBeOfType<SocketTransport>();

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
            var existingSource = Mock.Of<IEndPointSource>();
            var existingTransport = Mock.Of<IStatsDTransport>();
            var existingPublisher = Mock.Of<IStatsDPublisher>();

            var provider = Configure(services =>
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

            var source = provider.GetRequiredService<IEndPointSource>();
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
            Should.NotThrow(() => Configure(services => services.AddStatsD()));
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

            var provider = Configure(services =>
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

            var source = provider.GetRequiredService<IEndPointSource>();
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

            var provider = Configure(services =>
                {
                    // Act
                    services.AddSingleton<IStatsDTransport>(
                        ctx => new SocketTransport(ctx.GetRequiredService<IEndPointSource>(), SocketProtocol.IP));
                    services.AddStatsD(host);
                });

            // Assert
            var configuration = provider.GetRequiredService<StatsDConfiguration>();
            configuration.ShouldNotBeNull();
            configuration.Host.ShouldBe(host);
            configuration.Prefix.ShouldBeEmpty();

            var source = provider.GetRequiredService<IEndPointSource>();
            source.ShouldNotBeNull();

            var transport = provider.GetRequiredService<IStatsDTransport>();
            transport.ShouldNotBeNull();
            transport.ShouldBeOfType<SocketTransport>();

            var publisher = provider.GetRequiredService<IStatsDPublisher>();
            publisher.ShouldNotBeNull();
            publisher.ShouldBeOfType<StatsDPublisher>();
        }

        [Fact]
        public static void AddStatsDThrowsIfServicesIsNull()
        {
            // Arrange
            IServiceCollection services = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>("services", () => services.AddStatsD());
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

#pragma warning disable CA1812 // Instantiated via DI
        private sealed class MyTransport : IStatsDTransport
        {

#pragma warning disable CA1801 // Used to validate that IEndPointSource is in DI
            public MyTransport(IEndPointSource endpointSource)
            {
            }
#pragma warning restore CA1801

            public void Send(in ArraySegment<byte> metrics)
            {
            }
        }
#pragma warning restore CA1812
    }
}
