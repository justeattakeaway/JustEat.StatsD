using System;
using System.Threading;
using Shouldly;
using Xunit;

namespace JustEat.StatsD.Extensions
{
    public static class SimpleTimerStatNameTests
    {
        [Fact]
        public static void DefaultIsToKeepStatName()
        {
            var publisher = new FakeStatsPublisher();

            using (var timer = publisher.StartTimer("initialStat"))
            {
                Delay();
            }

            PublisherAssertions.SingleStatNameIs(publisher, "initialStat");
        }

        [Fact]
        public static void CanChangeStatNameDuringOperation()
        {
            var publisher = new FakeStatsPublisher();

            using (var timer = publisher.StartTimer("initialStat"))
            {
                Delay();
                timer.Bucket = "changedValue";
            }

            PublisherAssertions.SingleStatNameIs(publisher, "changedValue");
        }

        [Fact]
        public static void StatNameCanBeAppended()
        {
            var publisher = new FakeStatsPublisher();

            using (var timer = publisher.StartTimer("Some."))
            {
                Delay();
                timer.Bucket += "More";
            }

            PublisherAssertions.SingleStatNameIs(publisher, "Some.More");
        }

        [Fact]
        public static void StatWithoutNameAtStartThrows()
        {
            var publisher = new FakeStatsPublisher();

            Assert.Throws<ArgumentNullException>(() => publisher.StartTimer(string.Empty));

            publisher.CallCount.ShouldBe(0);
            publisher.BucketNames.ShouldBeEmpty();
        }

        [Fact]
        public static void StatWithoutNameAtEndThrows()
        {
            var publisher = new FakeStatsPublisher();

            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var timer = publisher.StartTimer("valid.Stat"))
                {
                    Delay();
                    timer.Bucket = null;
                }
            });

            publisher.CallCount.ShouldBe(0);
            publisher.BucketNames.ShouldBeEmpty();
        }

        [Fact]
        public static void StatNameIsInitialValueWhenExceptionIsThrown()
        {
            var publisher = new FakeStatsPublisher();
            var failCount = 0;

            try
            {
                using (var timer = publisher.StartTimer("initialStat"))
                {
                    Fail();
                    timer.Bucket = "changedValue";
                }
            }
            catch (Exception)
            {
                failCount++;
            }

            failCount.ShouldBe(1);
            PublisherAssertions.SingleStatNameIs(publisher, "initialStat");
        }

        private static void Delay()
        {
            const int standardDelayMillis = 200;
            Thread.Sleep(standardDelayMillis);
        }

        private static void Fail()
        {
            throw new Exception("Deliberate fail");
        }
    }
}
