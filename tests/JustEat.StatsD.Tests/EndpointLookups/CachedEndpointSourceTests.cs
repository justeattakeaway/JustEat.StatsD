using System.Net;
using NSubstitute;

namespace JustEat.StatsD.EndpointLookups;

public static class CachedEndpointSourceTests
{
    [Fact]
    public static void CachedValueIsReturnedFromInner()
    {
        var inner = Substitute.For<IEndPointSource>();
        inner.GetEndpoint().Returns(MakeTestIpEndPoint());

        var cachedEndpoint = new CachedEndpointSource(inner, TimeSpan.FromMinutes(5));

        var value = cachedEndpoint.GetEndpoint();
        value.ShouldNotBeNull();
        value.ShouldBe(MakeTestIpEndPoint());

        inner.Received(1).GetEndpoint();
    }

    [Fact]
    public static void CachedValueIsReturnedOnce()
    {
        var inner = Substitute.For<IEndPointSource>();
        inner.GetEndpoint().Returns(MakeTestIpEndPoint());

        var cachedEndpoint = new CachedEndpointSource(inner, TimeSpan.FromMinutes(5));

        var value1 = cachedEndpoint.GetEndpoint();
        var value2 = cachedEndpoint.GetEndpoint();
        var value3 = cachedEndpoint.GetEndpoint();

        value1.ShouldNotBeNull();
        value1.ShouldBe(value2);
        value1.ShouldBe(value3);

        inner.Received(1).GetEndpoint();
    }

    [Fact]
    public static async Task CachedValueIsReturnedAgainAfterExpiry()
    {
        var inner = Substitute.For<IEndPointSource>();
        inner.GetEndpoint().Returns(MakeTestIpEndPoint());

        var cachedEndpoint = new CachedEndpointSource(inner, TimeSpan.FromSeconds(1));

        cachedEndpoint.GetEndpoint();
        cachedEndpoint.GetEndpoint();

        await Task.Delay(1500);

        cachedEndpoint.GetEndpoint();
        cachedEndpoint.GetEndpoint();

        inner.Received(2).GetEndpoint();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public static void ConstructorThrowsIfCacheDurationIsInvalid(long ticks)
    {
        // Arrange
        var inner = Substitute.For<IEndPointSource>();
        var cacheDuration = TimeSpan.FromTicks(ticks);

        // Act and Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>("cacheDuration", () => new CachedEndpointSource(inner, cacheDuration));

        exception.ActualValue.ShouldBe(cacheDuration);
    }

    [Fact]
    public static void ConstructorThrowsIfInnerIsNull()
    {
        // Arrange
        IEndPointSource? inner = null;
        var cacheDuration = TimeSpan.FromHours(1);

        // Act and Assert
        Assert.Throws<ArgumentNullException>("inner", () => new CachedEndpointSource(inner!, cacheDuration));
    }

    private static IPEndPoint MakeTestIpEndPoint()
    {
        return new IPEndPoint(new IPAddress(new byte[] { 1, 2, 3, 4 }), 8125);
    }
}
