using System;
using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// A class representing an implementation of <see cref="IEndPointSource"/> that caches
    /// the <see cref="EndPoint"/> for a fixed period of time before refreshing its value.
    /// </summary>
    public class CachedEndpointSource : IEndPointSource
    {
        private readonly IEndPointSource _inner;
        private readonly TimeSpan _cacheDuration;
        private EndPoint _cachedValue;
        private DateTime _expiry;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedEndpointSource"/> class.
        /// </summary>
        /// <param name="inner">The inner <see cref="IEndPointSource"/> to use.</param>
        /// <param name="cacheDuration">The duration values should be cached for.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="inner"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="cacheDuration"/> is less than or equal to <see cref="TimeSpan.Zero"/>.
        /// </exception>
        public CachedEndpointSource(IEndPointSource inner, TimeSpan cacheDuration)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));

            if (cacheDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(cacheDuration), cacheDuration, "The end point cache duration must be a positive TimeSpan value.");
            }

            _cachedValue = null;
            _cacheDuration = cacheDuration;
        }

        /// <inheritdoc />
        public EndPoint GetEndpoint()
        {
            if (NeedsRead())
            {
                _cachedValue = _inner.GetEndpoint();
                _expiry = DateTime.UtcNow.Add(_cacheDuration);
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
