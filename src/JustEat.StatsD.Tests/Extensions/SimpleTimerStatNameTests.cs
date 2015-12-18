using System;
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

            AssertStatNameIs(publisher, "initialStat");
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

            AssertStatNameIs(publisher, "changedValue");
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

            AssertStatNameIs(publisher, "Some.More");
        }


        [Test]
        public void StatWithoutNameAtStartThrows()
        {
            var publisher = new FakeStatsPublisher();

            Assert.Throws<ArgumentNullException>(() => publisher.StartTimer(string.Empty));

            Assert.That(publisher.CallCount, Is.EqualTo(0));
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
            Assert.That(publisher.BucketNames, Is.Empty);
        }


        [Test]
        public void StatNameIsInitialValueWhenExceptionIsThrown()
        {
            var publisher = new FakeStatsPublisher();
            var failCount = 0;
            try
            {
                using (var timer = publisher.StartTimer("initialStat"))
                {
                    Fail();
                    timer.StatName = "changedValue";
                }
            }
            catch (Exception)
            {
                failCount++;
            }

            Assert.That(failCount, Is.EqualTo(1));
            AssertStatNameIs(publisher, "initialStat");
        }

        private void Delay()
        {
            const int standardDelayMillis = 200;
            Thread.Sleep(standardDelayMillis);
        }

        private void Fail()
        {
            throw new Exception("Deliberate fail");
        }

        private void AssertStatNameIs(FakeStatsPublisher publisher, string statName)
        {
            Assert.That(publisher.CallCount, Is.EqualTo(1));
            Assert.That(publisher.DisposeCount, Is.EqualTo(0));

            Assert.That(publisher.BucketNames.Count, Is.EqualTo(1));
            Assert.That(publisher.BucketNames[0], Is.EqualTo(statName));
        }
    }
}
