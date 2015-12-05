using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace JustEat.StatsD.Tests.Extensions
{
    [TestFixture]
    public class ExtensionsCancelTests
    {
        [Test]
        public void DefaultIsNotToCancel()
        {
            var publisher = new FakeStatsPublisher();

            using (var timer = publisher.StartTimer("stat"))
            {
                Delay();
            }

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "stat" }));
        }

        [Test]
        public void CanCancelStat()
        {
            var publisher = new FakeStatsPublisher();

            using (var timer = publisher.StartTimer("stat"))
            {
                Delay();
                timer.Cancel();
            }

            Assert.That(publisher.CallCount, Is.EqualTo(0));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.Empty);
        }

        private void Delay()
        {
            const int standardDelayMillis = 200;
            Thread.Sleep(standardDelayMillis);
        }
    }
}
