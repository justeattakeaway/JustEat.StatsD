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

        public void Send(in ArraySegment<byte> metric)
        {
            throw new SocketException(42);
        }
    }
    
    public class WhenTheTransportThrows
    {
        [Theory]
        [MemberData(nameof(Publishers))]
        public void DefaultConfigurationSwallowsThrownExceptions(string name, Func<StatsDConfiguration, IStatsDPublisher> factory)
        {
            var validConfig = MakeValidConfig();

            var publisher = factory(validConfig);

            publisher.Increment("anyStat");
        }

        [Theory]
        [MemberData(nameof(Publishers))]
        public void NullErrorHandlerSwallowsThrownExceptions(string name, Func<StatsDConfiguration, IStatsDPublisher> factory)
        {
            var validConfig = MakeValidConfig();
            validConfig.OnError = null;

            var publisher = factory(validConfig);

            publisher.Increment("anyStat");
        }

        [Theory]
        [MemberData(nameof(Publishers))]
        public void TrueReturningErrorHandlerSwallowsThrownExceptions(string name, Func<StatsDConfiguration, IStatsDPublisher> factory)
        {
            var validConfig = MakeValidConfig();
            validConfig.OnError = e => true;

            var publisher = factory(validConfig);

            publisher.Increment("anyStat");
        }

        [Theory]
        [MemberData(nameof(Publishers))]
        public void FalseReturningErrorHandlerThrowsExceptions(string name, Func<StatsDConfiguration, IStatsDPublisher> factory)
        {
            var validConfig = MakeValidConfig();
            validConfig.OnError = e => false;

            var publisher = factory(validConfig);

            Should.Throw<SocketException>(() =>
                publisher.Increment("anyStat"));
        }

        [Theory]
        [MemberData(nameof(Publishers))]
        public void ThrownExceptionCanBeCaptured(string name, Func<StatsDConfiguration, IStatsDPublisher> factory)
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

        public static TheoryData<string, Func<StatsDConfiguration, IStatsDPublisher>> Publishers =>
            new TheoryData<string, Func<StatsDConfiguration, IStatsDPublisher>>
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
                "StatsDPublisherBuffered",
                config =>
                {
                    return new StatsDPublisher(config, new ThrowingTransport());
                }
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
