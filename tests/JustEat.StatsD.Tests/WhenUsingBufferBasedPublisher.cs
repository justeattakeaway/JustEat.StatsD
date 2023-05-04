using JustEat.StatsD.Buffered;

namespace JustEat.StatsD;

public class WhenUsingBufferBasedPublisher
{
    [Theory]
    [InlineData('x', 128)]
    [InlineData('x', 256)]
    [InlineData('x', 512)]
    [InlineData('x', 1024)]
    [InlineData('x', 2048)]
    [InlineData('x', 4096)]
    [InlineData('Ж', 128)]
    [InlineData('Ж', 256)]
    [InlineData('Ж', 512)]
    [InlineData('Ж', 1024)]
    [InlineData('Ж', 2048)]
    [InlineData('Ж', 4096)]
    public void ItShouldBeAbleToSendMessagesOfArbitraryLength(char ch, int count)
    {
        var config = new StatsDConfiguration
        {
            Host = "127.0.0.1"
        };

        var fakeTransport = new FakeTransport();
        var publisher = new BufferBasedStatsDPublisher(config, fakeTransport);
        publisher.Increment(new string(ch, count));
        fakeTransport.TimesCalled.ShouldBe(1);
    }

    private sealed class FakeTransport : IStatsDTransport
    {
        public int TimesCalled { get; private set; }

        public void Send(in ArraySegment<byte> metric)
        {
            TimesCalled++;
        }
    }
}
