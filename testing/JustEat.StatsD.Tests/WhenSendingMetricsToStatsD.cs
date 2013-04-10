using System.Collections.Generic;
using JustEat.Testing;
using NUnit.Framework;
using Rhino.Mocks;
using Shouldly;

namespace JustEat.StatsD.Tests
{
    [Ignore]
    public class WhenSendingMetricsToStatsD : BehaviourTest<StatsDUdpClient>
    {
        private IEnumerable<string> _metricToSend;
        private bool _result;


        protected override void Given()
        {
            _metricToSend = new string[1] {"test-bucket:100|c"};

            Mock<IStatsDUdpClient>().Expect(x => x.Send(Arg<IEnumerable<string>>.Is.NotNull)).Return(true);
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
