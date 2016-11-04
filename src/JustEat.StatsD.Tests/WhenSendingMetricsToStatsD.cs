using System.Collections.Generic;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using JustBehave;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Shouldly;

namespace JustEat.StatsD.Tests
{
    [Ignore("The reason for this test being ignored is lost in the mists of time.")]
    public class WhenSendingMetricsToStatsD : BehaviourTest<StatsDUdpClient>
    {
        private IEnumerable<string> _metricToSend;
        private bool _result;

        protected override void Given()
        {
            _metricToSend = new string[1] {"test-bucket:100|c"};
            Fixture.Freeze<IStatsDUdpClient>().CallsTo(x => x.Send(A<IEnumerable<string>>._)).Returns(true);
        }

        protected override void When()
        {
            _result = SystemUnderTest.Send(_metricToSend);
            SystemUnderTest.Dispose();
        }

        [Then]
        public void TheMetricShouldGetSent()
        {
            _result.ShouldBe(true);
        }
    }
}
