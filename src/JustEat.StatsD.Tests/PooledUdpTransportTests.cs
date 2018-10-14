using System;
using Xunit;

namespace JustEat.StatsD
{
    public static class PooledUdpTransportTests
    {
        [Fact]
        public static void Finalizer_Does_Not_Throw_If_Socket_Connect_Fails()
        {
            // Arrange
            var configuration = new StatsDConfiguration()
            {
                Host = "0",
            };

            var endPointSource = EndpointLookups.EndpointParser.MakeEndPointSource(
                configuration.Host,
                configuration.Port,
                configuration.DnsLookupInterval);

            using (var transport = new PooledUdpTransport(endPointSource))
            {
                try
                {
                    // Act - Constructor of ConnectedSocketPool will throw
                    transport.Send("foo");
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            // Assert - Force the finalizers to run, ~ConnectedSocketPool would
            // throw a NullReferenceException and terminate the process.
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
