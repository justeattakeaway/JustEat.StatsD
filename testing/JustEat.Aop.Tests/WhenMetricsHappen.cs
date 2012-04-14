using System.Globalization;
using JustEat.Testing;
using Shouldly;

namespace JustEat.Aop.Tests
{
	public class WhenMetricsHappen : BehaviourTest<StatsDPipe>
	{
		private readonly IUdpClient _client;
		private readonly string _metricName;

		protected override void CreateSystemUnderTest()
		{
			SystemUnderTest = new StatsDPipe(_client);
		}

		protected override void Given()
		{
			_client = Mock<IUdpClient>();
			_metricName = string.Format(CultureInfo.CurrentCulture, "unit-test.{0}", GetType().Name);
		}

		protected override void When()
		{
			SystemUnderTest.Increment(_metricName);
		}

		[Then]
		public void NoExceptionsShouldBeThrown()
		{
			ThrownException.ShouldBeNull();
		}

		[Then]
		public void TransportShouldReceiveMetric() {
			var data = string.Fromat("{0}|c", _metricName);
			Mock<IUdpClient>().AssertWasCalled(x => x.Send(data))
		}
	}
}
