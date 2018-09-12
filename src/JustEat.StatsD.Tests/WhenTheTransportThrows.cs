using System;
using System.Net.Sockets;
using JustEat.StatsD.Buffered;
using Shouldly;
using Xunit;

#pragma warning disable xUnit1026 // Theory methods should use all of their parameters disabled to render test case name

namespace JustEat.StatsD
{
    public class ThrowingTransport : IStatsDTransport, IStatsDBufferedTransport
    {
        public void Send(string metric)
        {
            throw new SocketException(42);
        }

        public void Send(ArraySegment<byte> metric)
        {
            throw new SocketException(42);
        }
    }
    
    public delegate IStatsDPublisher Factory(StatsDConfiguration config);

    public class WhenTheTransportThrows
    {
        [Theory]
        [MemberData(nameof(Publishers))]
        public void DefaultConfigurationSwallowsThrownExceptions(string name, Factory factory)
        {
            var validConfig = MakeValidConfig();

            var publisher = factory(validConfig);

            publisher.Increment("anyStat");
        }

        [Theory]
        [MemberData(nameof(Publishers))]
        public void NullErrorHandlerSwallowsThrownExceptions(string name, Factory factory)
        {
            var validConfig = MakeValidConfig();
            validConfig.OnError = null;

            var publisher = factory(validConfig);

            publisher.Increment("anyStat");
        }

        [Theory]
        [MemberData(nameof(Publishers))]
        public void TrueReturningErrorHandlerSwallowsThrownExceptions(string name, Factory factory)
        {
            var validConfig = MakeValidConfig();
            validConfig.OnError = e => true;

            var publisher = factory(validConfig);

            publisher.Increment("anyStat");
        }

        [Theory]
        [MemberData(nameof(Publishers))]
        public void FalseReturningErrorHandlerThrowsExceptions(string name, Factory factory)
        {
            var validConfig = MakeValidConfig();
            validConfig.OnError = e => false;

            var publisher = factory(validConfig);

            Should.Throw<SocketException>(() =>
                publisher.Increment("anyStat"));
        }

        [Theory]
        [MemberData(nameof(Publishers))]
        public void ThrownExceptionCanBeCaptured(string name, Factory factory)
        {
            var validConfig = MakeValidConfig();
            Exception capturedEx = null; 
            validConfig.OnError = e =>
                {
                    capturedEx = e;
                    return true;
                };

            var publisher = factory(validConfig);

            capturedEx.ShouldBeNull();
            publisher.Increment("anyStat");
            capturedEx.ShouldNotBeNull();
        }

        public static TheoryData<string, Factory> Publishers => new TheoryData<string, Factory>
        {
            {
                "BufferBasedStatsDPublisher",
                config => new BufferBasedStatsDPublisher(config, new ThrowingTransport())
            },
            {
                "StringBasedStatsDPublisher",
                config => new StringBasedStatsDPublisher(config, new ThrowingTransport())
            },
            {
                "StatsDPublisher",
                config => new StatsDPublisher(config, new ThrowingTransport())
            },
            {
                "StatsDPublisherBuffered",
                config => new StatsDPublisher(config, new ThrowingTransport(), true)
            }
        };

        private static StatsDConfiguration MakeValidConfig()
        {
            return new StatsDConfiguration
            {
                Host = "someserver.somewhere.com"
            };
        }
    }
}
