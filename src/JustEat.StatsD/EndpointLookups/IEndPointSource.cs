using System.Net;

namespace JustEat.StatsD.EndpointLookups;

/// <summary>
/// Defines a method for retrieving the endpoint for a StatsD server.
/// </summary>
public interface IEndPointSource
{
    /// <summary>
    /// Returns an <see cref="EndPoint"/> to use for a StatsD server.
    /// </summary>
    /// <returns>
    /// The <see cref="EndPoint"/> to use to publish metrics.
    /// </returns>
    EndPoint GetEndpoint();
}
