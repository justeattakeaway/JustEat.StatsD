using System.Globalization;
using JustEat.Aop;
using JustEat.Testing;
using Rhino.Mocks;
using Shouldly;

namespace JustEat.Aop.Tests
{
	public class WhenMetricsHappen : BehaviourTest<StatsDPipe>
	{
		private IUdpClient _client;
		private string _metricName;

		protected override StatsDPipe CreateSystemUnderTest()
		{
			return new StatsDPipe(_client);
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
			ThrownException.ShouldBe(null);
		}

		[Then]
		public void TransportShouldReceiveMetric()
		{
			Mock<IUdpClient>().AssertWasCalled(x => x.Send(Arg<byte[]>.Is.NotNull, Arg<int>.Is.Anything));
		}
	}
}
