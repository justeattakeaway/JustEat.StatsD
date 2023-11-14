using System.Net;
using System.Net.Sockets;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD;

/// <summary>
/// A class representing an implementation of <see cref="IStatsDTransport"/>
/// that uses UDP or IP sockets and pools them.
/// This class cannot be inherited.
/// </summary>
public sealed class SocketTransport : IStatsDTransport, IDisposable
{
    private readonly IEndPointSource _endpointSource;
    private readonly SocketProtocol _socketProtocol;
    private ConnectedSocketPool? _pool;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocketTransport"/> class.
    /// </summary>
    /// <param name="endPointSource">The <see cref="IEndPointSource"/> to use.</param>
    /// <param name="socketProtocol">Udp or Ip sockets</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="endPointSource"/> is <see langword="null"/>.
    /// </exception>
    public SocketTransport(IEndPointSource endPointSource, SocketProtocol socketProtocol)
    {
        _endpointSource = endPointSource ?? throw new ArgumentNullException(nameof(endPointSource));

        if (!Enum.IsDefined(typeof(SocketProtocol), socketProtocol))
        {
            throw new ArgumentOutOfRangeException(nameof(socketProtocol), socketProtocol, $"Invalid {nameof(SocketProtocol)} value specified.");
        }
        _socketProtocol = socketProtocol;
    }

    /// <inheritdoc />
    public void Send(in ArraySegment<byte> metric)
    {
        if (metric.Array == null || metric.Count == 0)
        {
            return;
        }

        var endpoint = _endpointSource.GetEndpoint();
        if (endpoint == null)
        {
            return;
        }

        var pool = GetPool(endpoint);
        var socket = pool.PopOrCreate();

        try
        {
            socket.Send(metric.Array, 0, metric.Count, SocketFlags.None);
        }
        catch (Exception)
        {
            socket.Dispose();
            throw;
        }

        pool.Push(socket);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _pool?.Dispose();
        _pool = null;
    }

    private ConnectedSocketPool GetPool(EndPoint endPoint)
    {
        var oldPool = _pool;

        if (oldPool != null && (ReferenceEquals(oldPool.EndPoint, endPoint) || oldPool.EndPoint.Equals(endPoint)))
        {
            return oldPool;
        }

        var newPool = new ConnectedSocketPool(
            endPoint, _socketProtocol, Environment.ProcessorCount);

        if (Interlocked.CompareExchange(ref _pool, newPool, oldPool) == oldPool)
        {
            oldPool?.Dispose();
            return newPool;
        }
        else
        {
            newPool.Dispose();
            return _pool!;
        }
    }
}
