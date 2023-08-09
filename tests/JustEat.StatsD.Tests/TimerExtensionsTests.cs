using NSubstitute;

namespace JustEat.StatsD;

public static class TimerExtensionsTests
{
    [Fact]
    public static void StartTimerThrowsIfPublisherIsNull()
    {
        // Arrange
        IStatsDPublisher? publisher = null;
        string bucket = "bucket";

        // Act and Assert
        Assert.Throws<ArgumentNullException>("publisher", () => publisher!.StartTimer(bucket));
    }

    [Fact]
    public static void TimeThrowsIfActionIsNull()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        Action? action = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>("action", () => publisher.Time(bucket, action!));
    }

    [Fact]
    public static void TimeThrowsIfActionForTimerIsNull()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        Action<IDisposableTimer>? action = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>("action", () => publisher.Time(bucket, action!));
    }

    [Fact]
    public static async Task TimeThrowsIfFuncForTaskIsNull()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        Func<Task>? action = null;

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>("action", () => publisher.Time(bucket, action!));
    }

    [Fact]
    public static async Task TimeThrowsIfFuncForTaskWithTimerIsNull()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        Func<IDisposableTimer, Task>? action = null;

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>("action", () => publisher.Time(bucket, action!));
    }

    [Fact]
    public static void TimeThrowsIfFuncOfTIsNull()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        Func<int>? func = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>("func", () => publisher.Time(bucket, func!));
    }

    [Fact]
    public static void TimeThrowsIfFuncOfTWithTimerIsNull()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        Func<IDisposableTimer, int>? func = null;

        // Act and Assert
        Assert.Throws<ArgumentNullException>("func", () => publisher.Time(bucket, func!));
    }

    [Fact]
    public static async Task TimeThrowsIfFuncForTaskOfTIsNull()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        Func<Task<int>>? func = null;

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>("func", () => publisher.Time(bucket, func!));
    }

    [Fact]
    public static async Task TimeThrowsIfFuncForTaskOfTWithTimerIsNull()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        Func<IDisposableTimer, Task<int>>? func = null;

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>("func", () => publisher.Time(bucket, func!));
    }

    [Fact]
    public static void CanTimeAction()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        bool timed = false;

        // Act
        publisher.Time(bucket, () => { timed = true; });

        // Assert
        timed.ShouldBeTrue();
        publisher.Received(1).Timing(Arg.Any<long>(), 1, bucket, Arg.Any<Dictionary<string, string?>>());
    }

    [Fact]
    public static void CanTimeActionWithTimerOfT()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        // Act
        int actual = publisher.Time(
            bucket,
            (timer) =>
            {
                timer.ShouldNotBeNull();
                timer.Bucket.ShouldBe(bucket);

                return 42;
            });

        // Assert
        actual.ShouldBe(42);
        publisher.Received(1).Timing(Arg.Any<long>(), 1, bucket, Arg.Any<Dictionary<string, string?>>());
    }

    [Fact]
    public static async Task CanTimeAsyncActionWithTimerOfT()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        bool timed = false;

        // Act
        await publisher.Time(
            bucket,
            (timer) =>
            {
                timer.ShouldNotBeNull();
                timer.Bucket.ShouldBe(bucket);

                timed = true;

                return Task.CompletedTask;
            });

        // Assert
        timed.ShouldBeTrue();
        publisher.Received(1).Timing(Arg.Any<long>(), 1, bucket, Arg.Any<Dictionary<string, string?>>());
    }

    [Fact]
    public static async Task CanTimeAsyncFuncWithTimerOfT()
    {
        // Arrange
        var publisher = Substitute.For<IStatsDPublisher>();
        string bucket = "bucket";

        // Act
        int actual = await publisher.Time(
            bucket,
            (timer) =>
            {
                timer.ShouldNotBeNull();
                timer.Bucket.ShouldBe(bucket);

                return Task.FromResult(42);
            });

        // Assert
        actual.ShouldBe(42);
        publisher.Received(1).Timing(Arg.Any<long>(), 1, bucket, Arg.Any<Dictionary<string, string?>>());
    }
}
