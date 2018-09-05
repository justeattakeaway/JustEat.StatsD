using System;
using System.Text;
using JustEat.StatsD.V2;
using Xunit;

namespace JustEat.StatsD
{
    [CollectionDefinition("ActiveUdpListeners")]
    public class UdpListenersCollection : ICollectionFixture<UdpListeners>
    {
    }

    public class SomeTests
    {
        [Fact]
        public void Wat()
        {
            Span<byte> test = stackalloc byte[12];
            var bytes = Encoding.UTF8.GetBytes("Some really long string", test);
            Console.WriteLine(bytes);
        }

        [Fact]
        public void Shot()
        {
            var formatter = new StatsDUtf8Formatter("hello.world");
            var statsDMessage = StatsDMessage.Counter(77715155521, "some.really.long.stat.bucket");

            var buffer = new byte[512];
            formatter.TryFormat(statsDMessage, 0.5151, buffer, out var bytes);
            
            var text = Encoding.UTF8.GetString(buffer.AsSpan(0, bytes));

            Console.WriteLine(text);

        }
    }
}
