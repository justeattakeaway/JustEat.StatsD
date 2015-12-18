using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace JustEat.StatsD.Tests.Extensions
{
    [TestFixture]
    public class SimpleTimerStatNameTests
    {
        [Test]
        public void DefaultIsToKeepStatName()
        {
            var publisher = new FakeStatsPublisher();

            using (var timer = publisher.StartTimer("initialStat"))
            {
                Delay();
            }

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "initialStat" }));
        }

        [Test]
        public void CanChangeStatNameDuringOperation()
        {
            var publisher = new FakeStatsPublisher();

            using (var timer = publisher.StartTimer("initialStat"))
            {
                Delay();
                timer.StatName = "changedValue";
            }

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "changedValue" }));
        }

        [Test]
        public void StatNameCanBeAppended()
        {
            var publisher = new FakeStatsPublisher();

            using (var timer = publisher.StartTimer("Some."))
            {
                Delay();
                timer.StatName += "More";
            }

            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.EquivalentTo(new List<string> { "Some.More" }));
        }


        [Test]
        public void StatWithoutNameAtStartThrows()
        {
            var publisher = new FakeStatsPublisher();

            Assert.Throws<ArgumentNullException>(() => publisher.StartTimer(string.Empty));

            Assert.That(publisher.CallCount, Is.EqualTo(0));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));
            Assert.That(publisher.BucketNames, Is.Empty);
        }

        [Test]
        public void StatWithoutNameAtEndThrows()
        {
            var publisher = new FakeStatsPublisher();

            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var timer = publisher.StartTimer("valid.Stat"))
                {
                    Delay();
                    timer.StatName = null;
                }
            });

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
