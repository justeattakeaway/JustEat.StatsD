using System.Text;
using JustEat.StatsD.Buffered;

namespace JustEat.StatsD;

public class BufferBasedStatsDPublisherTests
{
    private readonly FakeTransport _transport = new();
    private readonly StatsDConfiguration _configuration = new() { Prefix = "test" };
    private readonly BufferBasedStatsDPublisher _sut;

    public BufferBasedStatsDPublisherTests()
    {
        _sut = new BufferBasedStatsDPublisher(_configuration, _transport);
    }

    [Fact]
    public void TestTiming_TimeSpan()
    {
        _sut.Timing(TimeSpan.FromSeconds(1.234), "timing");
        _transport.Messages.ShouldHaveSingleItem("test.timing:1234|ms");
    }

    [Fact]
    public void TestTimingSampled_TimeSpan()
    {
        for (int i = 0; _transport.Messages.Count == 0 && i < 10000; i++)
        {
            _sut.Timing(TimeSpan.FromSeconds(1.234), 0.99, "timing");
        }
        _transport.Messages.ShouldHaveSingleItem("test.timing:1234|ms|@0.99");
    }

    [Fact]
    public void TestTiming_Long()
    {
        _sut.Timing(1234, "timing");
        _transport.Messages.ShouldHaveSingleItem("test.timing:1234|ms");
    }

    [Fact]
    public void TestTimingSampled_Long()
    {
        for (int i = 0; _transport.Messages.Count == 0 && i < 10000; i++)
        {
            _sut.Timing(1234, 0.99, "timing");
        }
        _transport.Messages.ShouldHaveSingleItem("test.timing:1234|ms|@0.99");
    }

    private sealed class FakeTransport : IStatsDTransport
    {
        public List<string> Messages { get; } = new List<string>();

        public void Send(in ArraySegment<byte> metric)
        {
            Messages.Add(Encoding.UTF8.GetString(metric.Array!, metric.Offset, metric.Count));
        }
    }
}
