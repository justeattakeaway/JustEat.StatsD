using System;
using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// cache the IPEndPoint and only go to the source when it expires
    /// </summary>
    public class CachedIpEndpointSource : IPEndPointSource
    {
        private IPEndPoint _cachedValue;
        private DateTime _cachedAt;
        private readonly int _cacheDurationSeconds;

        private readonly IPEndPointSource _inner;

        public CachedIpEndpointSource(IPEndPointSource inner, int cacheDurationSeconds)
        {
            _inner = inner;
            _cacheDurationSeconds = cacheDurationSeconds;
        }

        public IPEndPoint Endpoint
        {
            get
            {
                if (NeedsRead())
                {
                    _cachedValue = _inner.Endpoint;
                    _cachedAt = DateTime.UtcNow;
                }
                return _cachedValue;
            }
        }

        private bool NeedsRead()
        {
            if (_cachedValue == null)
            {
                return true;
            }

            return _cachedAt.AddSeconds(_cacheDurationSeconds) <= DateTime.UtcNow;
        }
    }
}