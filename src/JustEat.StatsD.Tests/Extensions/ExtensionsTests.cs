using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JustEat.StatsD.Tests.Extensions
{
    [TestFixture]
    public class ExtensionsTests
    {
        private const int StandardDelayMillis = 200;

        [Test]
        public void CanRecordStat()
        {
            var publisher = new FakeStatsPublisher();

            using (publisher.StartTimer("stat"))
            {
                Delay();
            }

            PublisherAssertions.SingleStatNameIs(publisher, "stat");
            PublisherAssertions.LastDurationIs(publisher, StandardDelayMillis);
        }

        [Test]
        public void ShouldNotDisposePublisherAfterStatSent()
        {
            var publisher = new FakeStatsPublisher();

            using (publisher.StartTimer("stat"))
            {
                Delay();
            }

            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
        }

        [Test]
        public void CanRecordTwoStats()
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

            Assert.That(publisher.CallCount, Is.EqualTo(2));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat1", "stat2" }));
            PublisherAssertions.LastDurationIs(publisher, StandardDelayMillis);
        }

        [Test]
        public async Task CanRecordStatAsync()
        {
            var publisher = new FakeStatsPublisher();

            using (publisher.StartTimer("statWithAsync"))
            {
                await DelayAsync();
            }

            PublisherAssertions.SingleStatNameIs(publisher, "statWithAsync");
            PublisherAssertions.LastDurationIs(publisher, StandardDelayMillis);
        }

        [Test]
        public async Task CanRecordTwoStatsAsync()
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

            Assert.That(publisher.CallCount, Is.EqualTo(2));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat1", "stat2" }));
            PublisherAssertions.LastDurationIs(publisher, StandardDelayMillis);
        }

        [Test]
        public void CanRecordStatInAction()
        {
            var publisher = new FakeStatsPublisher();
            publisher.Time("statOverAction", () => Delay());

            PublisherAssertions.SingleStatNameIs(publisher, "statOverAction");
            PublisherAssertions.LastDurationIs(publisher, StandardDelayMillis);
        }

        [Test]
        public void CanRecordStatInFunction()
        {
            var publisher = new FakeStatsPublisher();
            var answer = publisher.Time("statOverFunc", () => DelayedAnswer());

            Assert.That(answer, Is.EqualTo(42));
            PublisherAssertions.SingleStatNameIs(publisher, "statOverFunc");
            PublisherAssertions.LastDurationIs(publisher, StandardDelayMillis);
        }

        [Test]
        public async Task CanRecordStatInAsyncAction()
        {
            var publisher = new FakeStatsPublisher();
            await publisher.Time("statOverAsyncAction", async () => await DelayAsync());

            PublisherAssertions.SingleStatNameIs(publisher, "statOverAsyncAction");
        }

        [Test]
        public async Task CorrectDurationForStatInAsyncAction()
        {
            var publisher = new FakeStatsPublisher();
            await publisher.Time("stat", async () => await DelayAsync());

            PublisherAssertions.LastDurationIs(publisher, StandardDelayMillis);
        }

        [Test]
        public async Task CanRecordStatInAsyncFunction()
        {
            var publisher = new FakeStatsPublisher();
            var answer = await publisher.Time("statOverAsyncFunc", async () => await DelayedAnswerAsync());

            Assert.That(answer, Is.EqualTo(42));
            PublisherAssertions.SingleStatNameIs(publisher, "statOverAsyncFunc");
        }

        [Test]
        public async Task CorrectDurationForStatInAsyncFunction()
        {
            var publisher = new FakeStatsPublisher();
            await publisher.Time("stat", async () => await DelayedAnswerAsync());

            PublisherAssertions.LastDurationIs(publisher, StandardDelayMillis);
        }

        private void Delay()
        {
            Thread.Sleep(StandardDelayMillis);
        }

        private async Task DelayAsync()
        {
            await Task.Delay(StandardDelayMillis);
        }

        private int DelayedAnswer()
        {
            Thread.Sleep(StandardDelayMillis);
            return 42;
        }

        private async Task<int> DelayedAnswerAsync()
        {
            await Task.Delay(StandardDelayMillis);
            return 42;
        }
    }
}
