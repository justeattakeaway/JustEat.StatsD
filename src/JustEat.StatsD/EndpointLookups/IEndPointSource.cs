using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    public interface IEndPointSource
    {
        EndPoint GetEndpoint();
    }
}
