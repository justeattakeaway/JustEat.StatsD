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
        private DateTime _expiry;
        private readonly int _cacheDurationSeconds;

        private readonly IPEndPointSource _inner;

        public CachedIpEndpointSource(IPEndPointSource inner, int cacheDurationSeconds)
        {
            _inner = inner;
            _cachedValue = null;
            _cacheDurationSeconds = cacheDurationSeconds;
        }

        public IPEndPoint GetEndpoint()
        {
            if (NeedsRead())
            {
                _cachedValue = _inner.GetEndpoint();
                _expiry = DateTime.UtcNow.AddSeconds(_cacheDurationSeconds);
            }
            return _cachedValue;
        }

        private bool NeedsRead()
        {
            if (_cachedValue == null)
            {
                return true;
            }

            return _expiry <= DateTime.UtcNow;
        }
    }
}