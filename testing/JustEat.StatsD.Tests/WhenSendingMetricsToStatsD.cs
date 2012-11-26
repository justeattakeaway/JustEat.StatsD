using JustEat.Testing;
using NUnit.Framework;
using Rhino.Mocks;
using Shouldly;

namespace JustEat.StatsD.Tests
{
	[Ignore]
	public class WhenSendingMetricsToStatsD : BehaviourTest<IStatsDUdpClient>
	{
		private string _metricToSend;
		private bool _result;
		private StatsDUdpClient _statsDUdpClient;

		private const string Host = "monitoring-test.je-labs.com";
		private const int Port = 8125;

		protected override void Given()
		{
			_metricToSend = "test-bucket:100|c";
			_statsDUdpClient = new StatsDUdpClient(Host,Port);

			Mock<StatsDUdpClient>().Stub(x => x.Send(_metricToSend)).Return(true);
			Mock<IStatsDUdpClient>().Expect(x => x.Send(Arg<string>.Is.NotNull)).Return(true);			
		}

		protected override void When()
		{
			_result = _statsDUdpClient.Send(_metricToSend); 
			_result = SystemUnderTest.Send(_metricToSend);
		}

		[Then]
		public void TheMetricShouldGetSent()
		{
			_result.ShouldBe(true);
		}
	}
}
