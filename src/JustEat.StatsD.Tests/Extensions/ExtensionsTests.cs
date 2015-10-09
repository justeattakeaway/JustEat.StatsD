using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JustEat.StatsD.Tests.Extensions
{
    [TestFixture]
    public class ExtensionsTests
    {
        [Test]
        public void CanRecordStat()
        {
            var publisher = new FakePublisher();

            using (publisher.StartTimer("stat"))
            {
                Delay();
            }

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
        }

        [Test]
        public void CanRecordTwoStats()
        {
            var publisher = new FakePublisher();

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
        }

        [Test]
        public async Task CanRecordStatAsync()
        {
            var publisher = new FakePublisher();

            using (publisher.StartTimer("stat"))
            {
                await DelayAsync();
            }

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
        }

        [Test]
        public async Task CanRecordTwoStatsAsync()
        {
            var publisher = new FakePublisher();

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
        }

        [Test]
        public void CanRecordStatInAction()
        {
            var publisher = new FakePublisher();
            publisher.Time("stat", () => Delay());

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
        }

        [Test]
        public void CanRecordStatInFunction()
        {
            var publisher = new FakePublisher();
            var answer = publisher.Time("stat", () => DelayedAnswer());

            Assert.That(answer, Is.EqualTo(42));
            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
        }

        [Test]
        public async Task CanRecordStatInAsyncAction()
        {
            var publisher = new FakePublisher();
            await publisher.Time("stat", async () => await DelayAsync());

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
        }

        [Test]
        public async Task CanRecordStatInAsyncFunction()
        {
            var publisher = new FakePublisher();
            var answer = await publisher.Time("stat", async () => await DelayedAnswerAsync());

            Assert.That(answer, Is.EqualTo(42));
            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
        }

        private void Delay()
        {
            Thread.Sleep(100);
        }

        private async Task DelayAsync()
        {
            await Task.Delay(100);
        }

        private int DelayedAnswer()
        {
            Thread.Sleep(100);
            return 42;
        }

        private async Task<int> DelayedAnswerAsync()
        {
            await Task.Delay(100);
            return 42;
        }
    }
}
