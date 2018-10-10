using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    public interface IPEndPointSource
    {
        EndPoint GetEndpoint();
    }
}
