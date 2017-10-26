using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Shouldly;
using Xunit;

namespace JustEat.StatsD
{
    public class ThrowingTransport : IStatsDTransport
    {
        public void Send(string metric)
        {
            Send(new[] {metric});
        }

        public void Send(IEnumerable<string> metrics)
        {
            throw new SocketException(42);
        }
    }

    public class WhenTheTransportThrows
    {
        [Fact]
        public void DefaultConfigurationSwallowsThrownExceptions()
        {
            var validConfig = MakeValidConfig();

            var publisher = MakeThrowingPublisher(validConfig);

            publisher.Increment("anyStat");
        }

        [Fact]
        public void NullErrorHandlerSwallowsThrownExceptions()
        {
            var validConfig = MakeValidConfig();
            validConfig.OnError = null;

            var publisher = MakeThrowingPublisher(validConfig);

            publisher.Increment("anyStat");
        }

        [Fact]
        public void TrueReturningErrorHandlerSwallowsThrownExceptions()
        {
            var validConfig = MakeValidConfig();
            validConfig.OnError = e => true;

            var publisher = MakeThrowingPublisher(validConfig);

            publisher.Increment("anyStat");
        }

        [Fact]
        public void FalseReturningErrorHandlerThrowsExceptions()
        {
            var validConfig = MakeValidConfig();
            validConfig.OnError = e => false;

            var publisher = MakeThrowingPublisher(validConfig);

            Should.Throw<SocketException>(() =>
                publisher.Increment("anyStat"));
        }

        [Fact]
        public void ThrownExceptionCanBeCaptured()
        {
            var validConfig = MakeValidConfig();
            Exception capturedEx = null; 
            validConfig.OnError = e =>
                {
                    capturedEx = e;
                    return true;
                };

            var publisher = MakeThrowingPublisher(validConfig);

            capturedEx.ShouldBeNull();
            publisher.Increment("anyStat");
            capturedEx.ShouldNotBeNull();
        }

        private static IStatsDPublisher MakeThrowingPublisher(StatsDConfiguration config)
        {
            return new StatsDPublisher(config, new ThrowingTransport());
        }

        private static StatsDConfiguration MakeValidConfig()
        {
            return new StatsDConfiguration
            {
                Host = "someserver.somewhere.com"
            };
        }
    }
}
