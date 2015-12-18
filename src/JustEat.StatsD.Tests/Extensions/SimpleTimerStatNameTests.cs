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

            PublisherAssertions.SingleStatNameIs(publisher, "initialStat");
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

            PublisherAssertions.SingleStatNameIs(publisher, "changedValue");
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

            PublisherAssertions.SingleStatNameIs(publisher, "Some.More");
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
            PublisherAssertions.SingleStatNameIs(publisher, "initialStat");
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
    }
}
