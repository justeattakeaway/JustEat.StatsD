using System;
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

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat" }));

            AssertDurationIsInExpectedRange(publisher.LastDuration);
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
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat1", "stat2" }));
            AssertDurationIsInExpectedRange(publisher.LastDuration);
        }

        [Test]
        public async Task CanRecordStatAsync()
        {
            var publisher = new FakeStatsPublisher();

            using (publisher.StartTimer("stat"))
            {
                await DelayAsync();
            }

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat" }));
            AssertDurationIsInExpectedRange(publisher.LastDuration);
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
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat1", "stat2" }));
            AssertDurationIsInExpectedRange(publisher.LastDuration);
        }

        [Test]
        public void CanRecordStatInAction()
        {
            var publisher = new FakeStatsPublisher();
            publisher.Time("stat", () => Delay());

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat" }));
            AssertDurationIsInExpectedRange(publisher.LastDuration);
        }

        [Test]
        public void CanRecordStatInFunction()
        {
            var publisher = new FakeStatsPublisher();
            var answer = publisher.Time("stat", () => DelayedAnswer());

            Assert.That(answer, Is.EqualTo(42));
            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat" }));
            AssertDurationIsInExpectedRange(publisher.LastDuration);
        }

        [Test]
        public async Task CanRecordStatInAsyncAction()
        {
            var publisher = new FakeStatsPublisher();
            await publisher.Time("stat", async () => await DelayAsync());

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat" }));
        }

        [Test]
        public async Task CorrectDurationForStatInAsyncAction()
        {
            var publisher = new FakeStatsPublisher();
            await publisher.Time("stat", async () => await DelayAsync());

            AssertDurationIsInExpectedRange(publisher.LastDuration);
        }

        [Test]
        public async Task CanRecordStatInAsyncFunction()
        {
            var publisher = new FakeStatsPublisher();
            var answer = await publisher.Time("stat", async () => await DelayedAnswerAsync());

            Assert.That(answer, Is.EqualTo(42));
            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat" }));
        }

        [Test]
        public async Task CorrectDurationForStatInAsyncFunction()
        {
            var publisher = new FakeStatsPublisher();
            await publisher.Time("stat", async () => await DelayedAnswerAsync());

            AssertDurationIsInExpectedRange(publisher.LastDuration);
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

        private void AssertDurationIsInExpectedRange(TimeSpan duration)
        {
            var expectedDelayLower = TimeSpan.FromMilliseconds(StandardDelayMillis);
            var expectedDelayUpper = TimeSpan.FromMilliseconds(StandardDelayMillis * 2);

            Assert.That(duration, Is.GreaterThanOrEqualTo(expectedDelayLower));
            Assert.That(duration, Is.LessThanOrEqualTo(expectedDelayUpper));

        }
    }
}
