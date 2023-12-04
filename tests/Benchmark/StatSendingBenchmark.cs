using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using JustEat.StatsD;
using JustEat.StatsD.Buffered;
using JustEat.StatsD.EndpointLookups;

namespace Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net50)]
[SimpleJob(RuntimeMoniker.Net60)]
public class StatSendingBenchmark : IDisposable
{
    private bool _disposed;
    private SocketTransport? _udpTransport;
    private BufferBasedStatsDPublisher? _udpSender;
    private SocketTransport? _ipTransport;
    private BufferBasedStatsDPublisher? _ipSender;
    private SocketTransport? _tcpTransport;
    private BufferBasedStatsDPublisher? _tcpSender;

    private static readonly TimeSpan Timed = TimeSpan.FromMinutes(1);

    ~StatSendingBenchmark()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _udpTransport?.Dispose();
                _ipTransport?.Dispose();
                _tcpTransport?.Dispose();
            }

            _disposed = true;
        }
    }

    [GlobalSetup]
    public void Setup()
    {
        var config = new StatsDConfiguration
        {
            // if you want to verify that stats are received,
            // you will need the IP of suitable local test stats server
            Host = "127.0.0.1",
            Prefix = "testmetric"
        };

        var ipConfig = new StatsDConfiguration
        {
            Host = config.Host,
            Prefix = config.Prefix,
            SocketProtocol = SocketProtocol.IP
        };
        var udpConfig = new StatsDConfiguration
        {
            Host = config.Host,
            Prefix = config.Prefix,
            SocketProtocol = SocketProtocol.Udp
        };
        var tcpConfig = new StatsDConfiguration
        {
            Host = config.Host,
            Prefix = config.Prefix,
            SocketProtocol = SocketProtocol.Tcp
        };

        var endpointSource = EndPointFactory.MakeEndPointSource(
            config.Host, config.Port, config.DnsLookupInterval);

        _ipTransport = new SocketTransport(endpointSource, SocketProtocol.IP);
        _ipSender = new BufferBasedStatsDPublisher(ipConfig, _ipTransport);
        _ipSender.Increment("startup.i");

        _udpTransport = new SocketTransport(endpointSource, SocketProtocol.Udp);
        _udpSender = new BufferBasedStatsDPublisher(udpConfig, _udpTransport);
        _udpSender.Increment("startup.u");

        _tcpTransport = new SocketTransport(endpointSource, SocketProtocol.Tcp);
        _tcpSender = new BufferBasedStatsDPublisher(tcpConfig, _tcpTransport);
        _tcpSender.Increment("startup.t");
    }


    [Benchmark]
    public void RunIp()
    {
        _ipSender!.Increment("hello.i");
        _ipSender!.Increment(20, "increment.i");
        _ipSender!.Timing(Timed, "timer.i");
        _ipSender!.Gauge(354654, "gauge.i");
        _ipSender!.Gauge(25.1, "free-space.i");
    }

    [Benchmark]
    public void RunIPWithSampling()
    {
        _ipSender!.Increment(2, 0.2, "increment.i");
        _ipSender!.Timing(2, 0.2, "increment.i");
    }

    [Benchmark]
    public void RunUdp()
    {
        _udpSender!.Increment("hello.u");
        _udpSender!.Increment(20, "increment.u");
        _udpSender!.Timing(Timed, "timer.u");
        _udpSender!.Gauge(354654, "gauge.u");
        _udpSender!.Gauge(25.1, "free-space.u");
    }

    [Benchmark]
    public void RunUdpWithSampling()
    {
        _udpSender!.Increment(2, 0.2, "increment.u");
        _udpSender!.Timing(2, 0.2, "increment.u");
    }

    [Benchmark]
    public void RunTcp()
    {
        _tcpSender!.Increment("hello.t");
        _tcpSender!.Increment(20, "increment.t");
        _tcpSender!.Timing(Timed, "timer.t");
        _tcpSender!.Gauge(354654, "gauge.t");
        _tcpSender!.Gauge(25.1, "free-space.t");
    }

    [Benchmark]
    public void RunTcpWithSampling()
    {
        _tcpSender!.Increment(2, 0.2, "increment.t");
        _tcpSender!.Timing(2, 0.2, "increment.t");
    }
}
