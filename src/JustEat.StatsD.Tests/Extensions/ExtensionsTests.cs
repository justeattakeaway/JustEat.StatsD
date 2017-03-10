using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace JustEat.StatsD.Extensions
{
    public static class ExtensionsTests
    {
        [Fact]
        public static void CanRecordStat()
        {
            var publisher = new FakeStatsPublisher();

            using (publisher.StartTimer("stat"))
            {
                Delay();
            }

            PublisherAssertions.SingleStatNameIs(publisher, "stat");
            PublisherAssertions.LastDurationIs(publisher, TimingConstants.DelayMilliseconds);
        }

        [Fact]
        public static void ShouldNotDisposePublisherAfterStatSent()
        {
            var publisher = new FakeStatsPublisher();

            using (publisher.StartTimer("stat"))
            {
                Delay();
            }

            publisher.DisposeCount.ShouldBe(0);
        }

        [Fact]
        public static void CanRecordTwoStats()
        {
            var publisher = new FakeStatsPublisher();

            using (publisher.StartTimer("stat1"))
            {
                Delay();
            }

            using (publisher.StartTimer("stat2"))
            {
                Delay();
            }

            publisher.CallCount.ShouldBe(2);
            publisher.BucketNames.ShouldBe(new[] { "stat1", "stat2" });
            PublisherAssertions.LastDurationIs(publisher, TimingConstants.DelayMilliseconds);
        }

        [Fact]
        public static async Task CanRecordStatAsync()
        {
            var publisher = new FakeStatsPublisher();

            using (publisher.StartTimer("statWithAsync"))
            {
                await DelayAsync();
            }

            PublisherAssertions.SingleStatNameIs(publisher, "statWithAsync");
            PublisherAssertions.LastDurationIs(publisher, TimingConstants.DelayMilliseconds);
        }

        [Fact]
        public static async Task CanRecordTwoStatsAsync()
        {
            var publisher = new FakeStatsPublisher();

            using (publisher.StartTimer("stat1"))
            {
                await DelayAsync();
            }

            using (publisher.StartTimer("stat2"))
            {
                await DelayAsync();
            }

            publisher.CallCount.ShouldBe(2);
            publisher.BucketNames.ShouldBe(new[] { "stat1", "stat2" });
            PublisherAssertions.LastDurationIs(publisher, TimingConstants.DelayMilliseconds);
        }

        [Fact]
        public static void CanRecordStatInAction()
        {
            var publisher = new FakeStatsPublisher();
            publisher.Time("statOverAction", () => Delay());

            PublisherAssertions.SingleStatNameIs(publisher, "statOverAction");
            PublisherAssertions.LastDurationIs(publisher, TimingConstants.DelayMilliseconds);
        }

        [Fact]
        public static void CanRecordStatInFunction()
        {
            var publisher = new FakeStatsPublisher();
            var answer = publisher.Time("statOverFunc", () => DelayedAnswer());

            answer.ShouldBe(42);
            PublisherAssertions.SingleStatNameIs(publisher, "statOverFunc");
            PublisherAssertions.LastDurationIs(publisher, TimingConstants.DelayMilliseconds);
        }

        [Fact]
        public static async Task CanRecordStatInAsyncAction()
        {
            var publisher = new FakeStatsPublisher();
            await publisher.Time("statOverAsyncAction", async () => await DelayAsync());

            PublisherAssertions.SingleStatNameIs(publisher, "statOverAsyncAction");
        }

        [Fact]
        public static async Task CorrectDurationForStatInAsyncAction()
        {
            var publisher = new FakeStatsPublisher();
            await publisher.Time("stat", async () => await DelayAsync());

            PublisherAssertions.LastDurationIs(publisher, TimingConstants.DelayMilliseconds);
        }

        [Fact]
        public static async Task CanRecordStatInAsyncFunction()
        {
            var publisher = new FakeStatsPublisher();
            var answer = await publisher.Time("statOverAsyncFunc", async () => await DelayedAnswerAsync());

            answer.ShouldBe(42);
            PublisherAssertions.SingleStatNameIs(publisher, "statOverAsyncFunc");
        }

        [Fact]
        public static async Task CorrectDurationForStatInAsyncFunction()
        {
            var publisher = new FakeStatsPublisher();
            await publisher.Time("stat", async () => await DelayedAnswerAsync());

            PublisherAssertions.LastDurationIs(publisher, TimingConstants.DelayMilliseconds);
        }

        private static void Delay()
        {
            Thread.Sleep(TimingConstants.DelayMilliseconds);
        }

        private static async Task DelayAsync()
        {
            await Task.Delay(TimingConstants.DelayMilliseconds);
        }

        private static int DelayedAnswer()
        {
            Thread.Sleep(TimingConstants.DelayMilliseconds);
            return 42;
        }

        private static async Task<int> DelayedAnswerAsync()
        {
            await Task.Delay(TimingConstants.DelayMilliseconds);
            return 42;
        }
    }
}
