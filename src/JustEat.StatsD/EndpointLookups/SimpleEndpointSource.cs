using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// A class representing an implementation of <see cref="IEndPointSource"/> that
    /// returns a constant <see cref="EndPoint"/> value. This class cannot be inherited.
    /// </summary>
    public sealed class SimpleEndpointSource : IEndPointSource
    {
        private readonly EndPoint _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleEndpointSource"/> class.
        /// </summary>
        /// <param name="value">The <see cref="EndPoint"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public SimpleEndpointSource(EndPoint value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public EndPoint GetEndpoint() => _value;
    }
}
