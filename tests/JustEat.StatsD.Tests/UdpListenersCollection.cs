using Xunit;

namespace JustEat.StatsD
{
    [CollectionDefinition("ActiveUdpListeners")]
    public class UdpListenersCollection : ICollectionFixture<UdpListeners>
    {
    }
}
